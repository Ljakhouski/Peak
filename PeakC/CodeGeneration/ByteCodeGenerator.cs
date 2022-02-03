using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;
namespace Peak.CodeGeneration
{
    class StackResult
    {
        public bool Nothing { get; set; }
        public SymbolType Result { get; set; }
    }
    class ByteCodeGenerator
    {
        private RuntimeModule currentModule;
        private SymbolTable globalTable;
        public ByteCodeGenerator()
        {

        }

        public RuntimeModule GetProgramRuntimeModule(ProgramNode programNode)
        {
            globalTable = new SymbolTable() { IsGlobalScopeTable = true };
            currentModule = new RuntimeModule();
            currentModule.Methods = new MethodDescription[1] { new MethodDescription() };
            generateForCodeBlock(programNode, globalTable);
            return currentModule;
        }

        private void generateForCodeBlock(Node node, SymbolTable currentSymbolTable)
        {
            if (node is ProgramNode)
            {
                foreach (Node n in ((ProgramNode)node).Node)
                {
                    if (n is LoadNode)
                    {
                        applyLoadNode((LoadNode)n, globalTable);
                    }
                    else if (n is VariableInitNode)
                    {
                        generateForVariable((VariableInitNode)n, globalTable);
                    }
                }
            }
        }

        private StackResult generateByteCode(Node node, SymbolTable currentSymbolTable)
        {
            if (node is ConstantNode)
            {
                var res = new StackResult() { Nothing = false };
                var byteCode = new List<Command>()
                    {
                        new Command(){ Name = CommandName.PushConst, Operands = new int[1]{
                            globalTable.GetConstantAddress(int.Parse((node as ConstantNode).Content.Content)) }
                        }
                    };
                addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], byteCode);
                //res.Result = new SymbolType() { Value = SymbolType.Type. };
                return res;
            }
            else
                throw new Exception();
        }
        private void applyLoadNode(LoadNode node, SymbolTable currentSymbolTable)
        {
            string fileName = (node as LoadNode).LoadFileName.Content;
            if (currentSymbolTable.IsNewFile(fileName))
            {
                currentSymbolTable.RegisterFile(fileName);
                var p = new Parser();
                this.generateForCodeBlock(p.GetNode(fileName), currentSymbolTable);

            }
            
        }

        private void generateForVariable(VariableInitNode n, SymbolTable currentSymbolTable)
        {
            if (currentSymbolTable.ContainsSymbol(n.Name))
            {
                Error.ErrMessage(n.Name, "name already exist");
            }
            else
            {
                if (n.Type == null)
                {
                    var type = generateByteCode(n.RightExpression, currentSymbolTable);
                    if (type.Nothing)
                        Error.ErrMessage(n.Name, "expression has no type");
                    currentSymbolTable.RegisterSymbol(new TableElement() { Type = type.Result, InfoNode = n, Name = n.Name.Content });
                }
            }
        }

        private void addByteCode(MethodDescription method, List<Command> newByteCode)
        {
            var newCodeArray = new Command[method.Code.Length + newByteCode.Count];
            currentModule.Methods[currentModule.Methods.Length - 1].Code.CopyTo(newCodeArray, 0);
            newByteCode.ToArray().CopyTo(newCodeArray, currentModule.Methods[currentModule.Methods.Length - 1].Code.Length);
            currentModule.Methods[currentModule.Methods.Length - 1].Code = newCodeArray;
        }

    }
}
