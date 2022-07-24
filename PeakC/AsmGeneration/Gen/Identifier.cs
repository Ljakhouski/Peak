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
                    return getVarRecursive(node, st, st, null /* send rbp */);

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

        private static GenResult getVarRecursive(IdentifierNode node, SymbolTable searchContext, SymbolTable st /* for code-gen and for stack/register managment*/, MemoryDataId framePointer)
        {
            // 1: take ref (where point on rbp in frame) on the next context
            // 2: if frame contains -> mov r?x, [ref+var_offset]
            // no? call GetVarRecursive() with ref.Context (it is the same context for var searching) and with ref (place of register with ref-address)

            var mRef = searchContext.GetMethodContextRef();

            if (mRef is null)
                Error.ErrMessage(node.Id, "name does not exist");
            else
            {
                if (mRef.Id.ExistInRegisters == false)
                {
                    var newRbpReg = st.MemoryAllocator.GetNewIdInRegister(); // this register will be free after finding
                  
                    var offset = mRef.Id.Rbp_Offset.ToString();

                    var reg1 = newRbpReg.Register.ToString();
                    var reg2 = framePointer is null ? "rbp" : framePointer.Register.ToString();
                    st.Emit(string.Format("mov {0}, [{1} {2}]", reg1, reg2, offset));
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
                    var e = mRef.Context.GetFromMethodContext(node.Id);

                    if (e is null == false && e is VariableTableElement) // if variable found
                    {
                        // mov r?x, [output + offset_2]
                        var outputDataRegisterId = st.MemoryAllocator.GetNewIdInRegister();
                        var offset_ = mRef.Id.Rbp_Offset;
                        st.Emit(string.Format("mov {0}, [{1} {2}]", outputDataRegisterId.Register, reg1, offset_));
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

                        st.MemoryAllocator.SetIdToFreeRegister((e as VariableTableElement).Id, outputDataRegisterId.Register);
                        //var outputRegisterId =  st.MemoryAllocator.ReserveFreeRegister(framePointer, st);
                        return new GenResult() { ResultType = e.Type, ReturnDataId = (e as VariableTableElement).Id };
                    }
                    else
                    {
                        if (searchContext is GlobalSymbolTable)
                            Error.ErrMessage(node.Id, "name does not exist");
                        else
                            return getVarRecursive(node, mRef.Context, st, newRbpReg /* output (then to take next frame-pointer inside, need add offset with output-register) */);
                    }
                }
                else
                    throw new CompileException(); // method-context-reference always place in stack
            }
            throw new CompileException();
        }
    }
}
