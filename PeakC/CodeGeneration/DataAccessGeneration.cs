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
        /*private GenerationResult generateIdAccessFromSymbolTable(Token name, SymbolTable currentSymbolTable)
        {
            SymbolTable currentContext = currentSymbolTable;

            while (true)
            {
                foreach (TableElement t in currentSymbolTable.Data)
                {
                    if (t.Name == name.Content)
                    {
                        //if (t.Type.Value == SymbolType.Type.Object) // for structures or namespaces

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

                /*if (currentContext.) // this is the function context and i can get a reference to another symbol-table with an access-code
                if (/*currentContext.Prev.IsGlobalScopeTable currentContext.IsGlobalScopeTable)
                {
                    Error.NameNotExistError(name);
                }
                /*else if (currentContext)
                else
                {
                    currentContext = currentContext.Prev;
                }
            }


        }*/

        private GenerationResult generateStoreData(Node node, SymbolTable currentSymbolTable)
        {
            if (node is IdentifierNode)
            {
                return generateStoreName((node as IdentifierNode).Id, currentSymbolTable);
            }
            else if (node is DotNode)
            {
                /* data-access-code-generation by dot-expression:
                    generate bytecode from right to left, last right node is store/push bytecode, all everything - data access (push)
                    
                 */
                throw new Exception();
            }
            /*else if (node is ArrayAccessNode)
            {
                var result = generatePushOnStackData(node.Left, currentSymbolTable);
                var indexResult = generatePushOnStackData(node.Index, currentSymbolTable);
            InstructionName.Store;
            }*/
            else
                Error.ErrMessage(node.MetaInf, "wrong expression");
            throw new Exception();
        }

        private GenerationResult generatePushOnStackData(Node node, SymbolTable currentSymbolTable)
        {
            if (node is IdentifierNode)
            {
                return generatePushOnStackName((node as IdentifierNode).Id, currentSymbolTable);
            }
            else if (node is DotNode)
            {
                /* data-access-code-generation by dot-expression:
                    generate bytecode from right to left, last right node is store/push bytecode, all everything - data access (push)
                    
                 */

                throw new Exception();
            }
            /*else if (node is ArrayAccessNode)
            {
                var result = generatePushOnStackData(node.Left, currentSymbolTable);
                var indexResult = generatePushOnStackData(node.Index, currentSymbolTable);
            InstructionName.Store;
            }*/
            else
                Error.ErrMessage(node.MetaInf, "wrong expression");
            throw new Exception();
        }

        private GenerationResult generateStoreName(Token name, SymbolTable currentSymbolTable)
        {
            if (currentSymbolTable.ContainsSymbol(name))
            {
                var element = currentSymbolTable.GetSymbol(name);

                if (currentSymbolTable.IsGlobalScopeTable)
                    addByteCode(currentSymbolTable.CurrentMethod, InstructionName.StoreGlobal, element.OffsetAddress);
                else
                    addByteCode(currentSymbolTable.CurrentMethod, InstructionName.Store, element.OffsetAddress);
                return new GenerationResult() { NameResult = element, Nothing = false, Result = element.Type };
            }
            else if (currentSymbolTable.IsGlobalScopeTable)
            {
                Error.NameNotExistError(name);
                throw new Exception();
            }
            else
            {
                if (currentSymbolTable.IsMethodScope)
                {
                    var table = generatePreviousFrameContextAccess(currentSymbolTable);

                    return generateStoreName(name, table);
                }
                /*else if (currentSymbolTable.IsStructScope)*/
                else
                {
                    return generateStoreName(name, currentSymbolTable.Prev);
                }
            }
        }

        private GenerationResult generatePushOnStackName(Token name, SymbolTable currentSymbolTable)
        {
            if (currentSymbolTable.ContainsSymbol(name))
            {
                var element = currentSymbolTable.GetSymbol(name);

                if (currentSymbolTable.IsGlobalScopeTable)
                    addByteCode(currentSymbolTable.CurrentMethod, InstructionName.PushGlobal, element.OffsetAddress);
                else
                    addByteCode(currentSymbolTable.CurrentMethod, InstructionName.Push, element.OffsetAddress);
                return new GenerationResult() { NameResult = element, Nothing = false, Result = element.Type };
            }
            else if (currentSymbolTable.IsGlobalScopeTable)
            {
                Error.NameNotExistError(name);
                throw new Exception();
            }
            else
            {
                if (currentSymbolTable.IsMethodScope)
                {
                    var table = generatePreviousFrameContextAccess(currentSymbolTable);

                    return generatePushOnStackName(name, table);
                }
                /*else if (currentSymbolTable.IsStructScope)*/
                else
                {
                    return generatePushOnStackName(name, currentSymbolTable.Prev);
                }
            }

        }
        private SymbolTable generatePreviousFrameContextAccess(SymbolTable currentSymbolTable)
        {
            foreach (TableElement element in currentSymbolTable.Data)
            {
                if (element.Type.Value == SymbolType.Type.RefOnMethodContext)
                {
                    var address = element.OffsetAddress;
                    addByteCode(currentSymbolTable.CurrentMethod, InstructionName.Push, address);
                    return element.MethodContextTable;
                }
            }
            throw new Exception();
            //Error.ErrMessage();
        }
    }
}
