using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class Identifier
    {
        /*  generate code to get access to the identifier (variable/const/method-oject/...) 
         *  data can be contains in the stack, then need to generate code to write 
         *  variable-data to the register or return information about variable-position
         *  (only for last method-context on the top-frame. 
         *  For using this data leter, need to move variable from stack to register ) 
         */

        public static GenResult Generate(IdentifierNode node, SymbolTable st)
        {
            var data = st.GetSymbolFromAllSpaces(node.Id);

            if (data is ConstTableElement)
            {
                return new ConstantResult()
                {
                    ResultType = data.Type, 
                    //IntValue = int.(data as ConstTableElement).ConstValue.Value,\
                    ConstValue = (data as ConstTableElement).ConstValue
                };
            }
            else if (data is VariableTableElement)
            {
                var v = st.GetFromMethodContext(node.Id);
                if (v is null)
                {
                    // search in other method-frames
                    return getVarRecursive(node, st, st, RegisterName.RBP);

                }
                else if (v is VariableTableElement) // if is variable in the top-local-frame
                {
                    var result = new ConstantResult()
                    {
                        ResultType = v.Type,
                        ReturnDataId = (v as VariableTableElement).Id
                    };
                    return result;
                }
            }
            else if (data is MethodTableElement)
            {
                Error.ErrMessage("method as variable not upported");
            }
            else if (data is null)
                Error.ErrMessage(node.MetaInf, "name does not exist");
            else
                throw new CompileException();
            throw new CompileException();
        }

        private static GenResult getVarRecursive(IdentifierNode node, SymbolTable searchContext, SymbolTable st /* for code-gen and for stack/register managment*/, RegisterName framePointer)
        {
            // 1: take ref on next context
            // 2: if frame contains -> mov r?x, [ref+var_offset]
            // no? call GetVarRecursive() with ref.Context (it is the same context for var searching) and with ref (place of register with ref-address)

            var mRef = searchContext.GetMethodContextRef();

            if (mRef is null)
                Error.ErrMessage(node.Id, "name does not exist");
            else
            {
                //var offset = st.MemoryAllocator.Find(mRef.Id).
                if (mRef.Id.ExistInRegisters == false)
                {
                    var outputRegister = st.MemoryAllocator.GetFreeRegister(); // this register will be free after finding

                    st.MethodCode.Emit(
                        InstructionName.Mov,
                        outputRegister,
                        new Operand()
                        {
                            IsGettingAddress = true,
                            RegisterName = framePointer,
                            Offset = st.MemoryAllocator.CalculateLocalOffset(mRef) 
                        });

                    var e = mRef.Context.GetFromMethodContext(node.Id);

                    if (e is null == false && e is VariableTableElement) // if variable found
                    {
                        // mov r?x, [output + offset_2]
                        st.MethodCode.Emit(
                            InstructionName.Mov,
                            outputRegister, // if it is the error, alloc new free register and then using it
                            new Operand()
                            {
                                IsGettingAddress = true,
                                RegisterName = outputRegister,
                                Offset = st.MemoryAllocator.CalculateLocalOffset(mRef)
                            });

                        st.MemoryAllocator.SetIdToFreeRegister((e as VariableTableElement).Id, outputRegister);
                        //var outputRegisterId =  st.MemoryAllocator.ReserveFreeRegister(framePointer, st);
                        return new GenResult() { ResultType = e.Type, ReturnDataId = (e as VariableTableElement).Id };
                    }
                    else
                    {
                        if (searchContext is GlobalSymbolTable)
                            Error.ErrMessage(node.Id, "name does not exist");
                        else
                            return getVarRecursive(node, mRef.Context, st, framePointer /* output (then to take next frame-pointer inside, need add offset with output-register) */);
                    }
                }
                else
                    throw new CompileException(); // method-context-reference always place in stack
                            }
            throw new CompileException();
        }
    }
}
