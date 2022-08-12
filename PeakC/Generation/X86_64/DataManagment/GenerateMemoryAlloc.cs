using Peak.PeakC.Generation.X86_64;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation
{
    partial class MemoryAllocator
    {
        public static void EmitMovRegisterToRegister(RegisterName from, RegisterName to, int size, SymbolTable st)
        {
            if (X86_64_Model.IsSSE(to))
            {
                throw new CompileException();
            }
            else
            {
                if (X86_64_Model.IsSSE(from) == false) // both is not SSE
                {
                    var r1Casted = X86_64_Model.CastRegister(to, size);
                    var r2Casted = X86_64_Model.CastRegister(from, size);

                    st.Emit($"mov {r1Casted}, {r2Casted}");
                }
                else
                    throw new CompileException();
            }
        }

        public static void EmitMovRegisterToMemory(RegisterName r, RegisterName address, int offset, int size, SymbolTable st)
        {
            var sizeOp = X86_64_Model.GetSizeName(size);

            if (X86_64_Model.IsSSE(r) == false)
            {
                var regCasted = X86_64_Model.CastRegister(r, size);
                st.Emit($"mov {sizeOp} [{address} {offset}], {regCasted}");
            }
            else
            {
                var regCasted = X86_64_Model.CastRegister(r, size);
                if (size == 8)
                    st.Emit($"movsd {sizeOp} [{address} {offset}], {regCasted}");
                else
                    throw new CompileException();
            }
        }

        public static void EmitMovFromMemoryToRegister(RegisterName address, int offset, RegisterName r, int size, SymbolTable st)
        {
            var sizeOp = X86_64_Model.GetSizeName(size);

            if (X86_64_Model.IsSSE(r) == false)
            {
                var regCasted = X86_64_Model.CastRegister(r, size);
                st.Emit($"mov {regCasted}, {sizeOp} [{address} {offset}]");
            }
            else
            {
                var regCasted = X86_64_Model.CastRegister(r, size);
                if (size == 8)
                    st.Emit($"movsd {regCasted}, {sizeOp} [{address} {offset}]");
                else
                    throw new CompileException();
            }
        }

        private static void generateMovToRegisterFromAllContexts(MemoryIdTracker id, SymbolTable st, RegisterName outputRegister)
        {
            var allocator = st.MemoryAllocator;

            allocator.Block(outputRegister);

            if (id.ExistInStack == false)
                throw new CompileException();

            // if (id is in GlobalScope)

            if (existHere(allocator))
            {
                EmitMovFromMemoryToRegister(RegisterName.rbp, id.Rbp_Offset, outputRegister, id.Size, st);
                                //allocator.SetRegister(id, outputRegister); // the variable-id-tracker can be in register, but can not be places in the stack again (as storage register)
                allocator.Unblock(outputRegister);
                return;
            }
            else
                genResursive(st, null /*rbp*/);

            /*********************************/
            /*  to search in other contexts  */
            /*********************************/

            void genResursive(SymbolTable context, MemoryIdTracker basePointer)
            {
                RegisterName basePointerRegister;
                if (basePointer != null)
                {
                    allocator.MoveToAnyRegister(basePointer);
                    basePointerRegister = basePointer.Register;
                }
                else
                    basePointerRegister = RegisterName.rbp;

                var contextAlloc = context.MemoryAllocator;
                var mRef = context.GetMethodContextRef();

                if (mRef is null)
                    throw new CompileException("func-contet-ref not working");

                // switch base pointer to point on the new frame (context)
                var newBasePointer = context.MemoryAllocator.RBP_dataId;
                var newBasePointerReigster = allocator.GetFreeRegister();
                EmitMovFromMemoryToRegister(basePointerRegister, newBasePointer.Rbp_Offset, newBasePointerReigster, 8, st);

                basePointer?.Free();

                if (existHere(contextAlloc))
                {
                    EmitMovFromMemoryToRegister(newBasePointer.Register, id.Rbp_Offset, outputRegister, id.Size, st);
                    allocator.SetRegister(id, outputRegister);
                    allocator.Unblock(outputRegister);
                    newBasePointer.Free();
                    return;
                }
                else
                    genResursive(mRef.Context, newBasePointer);
            }

            bool existHere(MemoryAllocator alloc_)
            {
                foreach (var e in alloc_.StackModel)
                    if (id == e.ContainedData)
                        return true;
                return false;
            }
        }
        // this method doesn't move trackers
        private static void generateMovRegisterToAnyContext(MemoryIdTracker place, RegisterName data, SymbolTable st)
        {
            var allocator = st.MemoryAllocator;

            allocator.Block(data);

            // TODO: if (id is in GlobalScope)

            if (existHere(allocator))
            {
                EmitMovRegisterToMemory(data, RegisterName.rbp, place.Rbp_Offset, place.Size, st);
                allocator.Unblock(data);
                return;
            }
            genResursive(st, null /*rbp*/);

            /*********************************/
            /*  to search in other contexts  */
            /*********************************/

            void genResursive(SymbolTable context, MemoryIdTracker basePointer)
            {
                RegisterName basePointerRegister;
                if (basePointer != null)
                {
                    allocator.MoveToAnyRegister(basePointer);
                    basePointerRegister = basePointer.Register;
                }
                else
                    basePointerRegister = RegisterName.rbp;

                var contextAlloc = context.MemoryAllocator;
                var mRef = context.GetMethodContextRef();

                if (mRef is null)
                    throw new CompileException("func-contet-ref not working");

                // switch base pointer to point on the new frame (context)
                var newBasePointer = context.MemoryAllocator.RBP_dataId;
                var newBasePointerReigster = allocator.GetFreeRegister();
                EmitMovFromMemoryToRegister(basePointerRegister, newBasePointer.Rbp_Offset, newBasePointerReigster, 8, st);

                basePointer?.Free();

                if (existHere(contextAlloc))
                {
                    EmitMovRegisterToMemory(data, newBasePointerReigster, place.Rbp_Offset, place.Size, st);
                    allocator.Unblock(data);
                    newBasePointer.Free();
                    return;
                }
                // if not found
                genResursive(mRef.Context, newBasePointer);
            }

            bool existHere(MemoryAllocator alloc_)
            {
                foreach (var e in alloc_.StackModel)
                    if (place == e.ContainedData)
                        return true;
                return false;
            }
        }
    }
}
