using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class SymbolTableSearching
    {
        public static GenResult GenerateGettingData(TableElement element, SymbolTable context, SymbolTable st)
        {

            var v = context.ExistInMethodContext(element);

            // search in local contexts: in the current-frame and or in other frames

            if (v == false)
            {
                // search in other method-frames
                return getVarRecursive(element, st, st, null /* send rbp */);
            }
            else
            {
                var outputId = st.MemoryAllocator.GetNewIdInRegister(size: 8);
                st.Emit($"mov {outputId.Register}, [rbp {getMemId(element).Rbp_Offset}]");
                var result = new GenResult()
                {
                    ResultType = element.Type,
                    ReturnDataId = outputId
                };
                return result;
            }
        }


        private static GenResult getVarRecursive(TableElement element, SymbolTable context, SymbolTable st /* for code-gen and for stack/register managment*/, MemoryIdTracker framePointer)
        {
            // 1: take ref (where point on rbp in frame) on the next context
            // 2: if frame contains -> mov r?x, [ref+var_offset]
            // no? call GetVarRecursive() with ref.Context (it is the same context for var searching) and with ref (place of register with ref-address)

            var mRef = context.GetMethodContextRef();

            if (mRef is null)
                throw new CompileException("func-contet-ref not working");
            else
            {
                if (mRef.MemoryId.ExistInRegisters == false)
                {
                    var newRbpReg = st.MemoryAllocator.GetNewIdInRegister(size: 8); // this register will be free after finding

                    var offset = mRef.MemoryId.Rbp_Offset.ToString();

                    var reg1 = newRbpReg.Register.ToString();
                    var reg2 = framePointer is null ? "rbp" : framePointer.Register.ToString();
                    st.Emit($"mov {reg1}, [{reg2} {offset}]");
                    framePointer.Free();
                    /*
                    st.MethodCode.Emit(
                        InstructionName.Mov,
                        outputRegister,
                        new Operand()
                        {
                            IsGettingAddress = true,
                            RegisterName = framePointer,
                            Offset = mRef.Id.Rbp_Offset 
                        });
                    */
                    var e = mRef.Context.ExistInMethodContext(element);

                    if (e) // if variable found in current new context
                    {
                        // mov r?x, [output + offset_2]
                        var outputDataRegisterId = st.MemoryAllocator.GetNewIdInRegister(getMemId(element).Size);
                        var offset_ = mRef.MemoryId.Rbp_Offset;
                        st.Emit($"mov {outputDataRegisterId.Register}, [{reg1} {offset_}]");
                        newRbpReg.Free();
                        /*
                        st.MethodCode.Emit(
                            InstructionName.Mov,
                            outputRegister, // if it is the error, alloc new free register and then using it
                            new Operand()
                            {
                                IsGettingAddress = true,
                                RegisterName = outputRegister,
                                Offset = mRef.Id.Rbp_Offset
                            });*/

                        st.MemoryAllocator.SetIdToFreeRegister(getMemId(element), outputDataRegisterId.Register);
                        //var outputRegisterId =  st.MemoryAllocator.ReserveFreeRegister(framePointer, st);
                        return new GenResult() { ResultType = element.Type, ReturnDataId = getMemId(element) };
                    }
                    else
                    {
                        if (context is GlobalSymbolTable)
                            throw new CompileException("table-element not exist");
                        else
                            return getVarRecursive(element, mRef.Context, st, newRbpReg /* output (then to take next frame-pointer inside, need add offset with output-register) */);
                    }
                }
                else
                    throw new CompileException(); // method-context-reference always place in stack
            }
            throw new CompileException();
        }

        /* set */
        public static void GenerateSettingData(TableElement element, MemoryIdTracker assignmentData, SymbolTable context, SymbolTable st)
        {
            var v = context.ExistInMethodContext(element);

            if (v)
            {
                st.MemoryAllocator.MoveToAnyRegister(assignmentData);

                var offset = getMemId(element).Rbp_Offset;
                var register = assignmentData.Register.ToString();

                st.Emit($"mov [rbp {offset}], {register}");
                //st.MemoryAllocator.SetId
                /*
                st.MethodCode.Emit(
                    new AsmInstruction()
                    {

                        InstructionName = InstructionName.Mov,

                        FirstOperand = new Operand()
                        {
                            IsGettingAddress = true,
                            RegisterName = RegisterName.RBP,
                            Offset = localResult.Id.StackOffset
                        },

                        SecondOperand = new Operand()
                        {
                            RegisterName = assignmentData.Register
                        }
                    });*/

                //assignmentData.Free(); why???
            }
            else
            {
                setVarRecursive(element, st, st, assignmentData, null /* send rbp */);
            }

        }

        private static void setVarRecursive(TableElement element, SymbolTable context, SymbolTable st, MemoryIdTracker assigmentData, MemoryIdTracker framePointer)
        {
            var contextRef = context.GetMethodContextRef();
            var newRbpRegId = st.MemoryAllocator.GetNewIdInRegister(size: 8);
            //st.MemoryAllocator.MoveToRegister(contextRef.Id);


            var reg1 = newRbpRegId.ToString();
            var reg2 = framePointer is null ? "rbp" : framePointer.Register.ToString();
            var offset = contextRef.MemoryId.Rbp_Offset;
            st.Emit($"mov {reg1}, [{reg2} {offset}]");
            if (framePointer is null == false)
                framePointer.Free();
            /*
            st.MethodCode.Emit(new AsmInstruction()
            {
                InstructionName = InstructionName.Mov,
                FirstOperand = new Operand()
                {
                    RegisterName = newRefRegister
                },
                SecondOperand = new Operand()
                {
                    IsGettingAddress = true,
                    RegisterName = stackAddressingRegister,
                    Offset =contextRef.Id.Rbp_Offset
                }
            });*/

            var e = contextRef.Context.ExistInMethodContext(element);

            
            if (e)
            {
                st.MemoryAllocator.MoveToAnyRegister(assigmentData);
                var offset_ = getMemId(element).Rbp_Offset;

                st.Emit($"mov [{newRbpRegId.Register} {offset}], {assigmentData.Register}");

                assigmentData.FreeFromRegister();
                // variable.Id = assigmentData.Id;
                /*st.MethodCode.Emit(new AsmInstruction()
                {
                    InstructionName = InstructionName.Mov,
                   
                    FirstOperand = new Operand()
                    {
                        IsGettingAddress = true,
                        RegisterName = newRefRegister,
                        Offset = variable.Id.Rbp_Offset
                    },

                    SecondOperand = new Operand()
                    {
                        RegisterName = variable.Id.Register
                    }
                });*/

            }
            else
            {
                if (context is GlobalSymbolTable)
                    throw new CompileException("does not exist");
                //Error.ErrMessage(ele, "name does not exist");
                else
                    setVarRecursive(element, contextRef.Context, st, assigmentData, framePointer);
            }            
        }
        private static MemoryIdTracker getMemId(TableElement e)
        {
            if (e is VariableTableElement)
                return (e as VariableTableElement).MemoryId;
            else if (e is MethodContextReferenceElement)
                return (e as MethodContextReferenceElement).MemoryId;
            else
                throw new CompileException("not implemented");
        }
    }
}
