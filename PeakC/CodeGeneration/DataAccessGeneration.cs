using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.CodeGeneration
{
    partial class ByteCodeGenerator
    {
        private GenerationResult generateDataAccess(Token name, SymbolTable currentSymbolTable)
        {
            SymbolTable currentContext = currentSymbolTable;

            while (true)
            {
                foreach (TableElement t in currentSymbolTable.Data)
                {
                    if (t.Name == name.Content)
                    {
                        if (currentContext.IsGlobalScopeTable)
                        {
                            var code = new List<Instruction>()
                            {
                                new Instruction(){ Name = InstructionName.PushStatic, Operands = new int[]{ t.OffsetAddress} }
                            };
                            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                        }
                        else
                        {
                            var code = new List<Instruction>()
                            {
                                new Instruction(){ Name = InstructionName.Push, Operands = new int[]{ t.OffsetAddress} }
                            };
                            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                        }
                        return new GenerationResult() { Nothing = false, Result = t.Type };
                    }
                }

                /*if (currentContext.)*/ // this is the function context and i can get a reference to another symbol-table with an access-code
                if (/*currentContext.Prev.IsGlobalScopeTable*/ currentContext.IsGlobalScopeTable)
                {
                    Error.ErrMessage(name, "name does not exist");
                }
                /*else if (currentContext)*/
                else
                {
                    currentContext = currentContext.Prev;
                }
            }








        }
    }
}
