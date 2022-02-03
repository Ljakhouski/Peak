using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;
namespace Peak.CodeGeneration
{
    class GenerationResult
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
            generateForProgramNode(programNode, globalTable);
            writeConstantSection();
            writeGlobalMemoryInfo();
            return currentModule;
        }

        private void writeConstantSection()
        {
            currentModule.Constant = new Constant[globalTable.ConstandData.Count];

            for (int i = 0; i < globalTable.ConstandData.Count; i++)
            {
                currentModule.Constant[i] = globalTable.ConstandData[i];
            }
        }

        private void writeGlobalMemoryInfo()
        {
            currentModule.Methods[0].LocalVarsArraySize = globalTable.MemorySize;
        }
        private void generateForProgramNode(ProgramNode node, SymbolTable currentSymbolTable)
        {
            foreach (Node n in ((ProgramNode)node).Node)
            {
                if (n is LoadNode)
                {
                    applyLoadNode((LoadNode)n, globalTable);
                }
                else
                    generateForCodeBlock((CodeBlockNode)n, currentSymbolTable);
            }
        }
        private void generateForCodeBlock(CodeBlockNode node, SymbolTable currentSymbolTable)
        {
            foreach (Node n in node.Node)
                if (n is VariableInitNode)
                {
                    generateForVariable((VariableInitNode)n, globalTable);
                }
                else if (n is ProgramNode)
                {

                }
                else
                    Error.ErrMessage(n.MetaInf, "expression is not supported in current context");
        }

        private GenerationResult generateByteCode(Node node, SymbolTable currentSymbolTable)
        {
            if (node is ConstValueNode)
            {
                return generationForConst(node as ConstValueNode, currentSymbolTable);
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
                this.generateForProgramNode(p.GetNode(fileName), currentSymbolTable);

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
                    else
                    {
                        currentSymbolTable.RegisterSymbol(new TableElement() { Type = type.Result, InfoNode = n, Name = n.Name.Content });
                        var res = generateForGetData(n.Name, currentSymbolTable);
                        if (res.Nothing || res.Result != type.Result)
                        {
                            throw new Exception();
                        }
                        else
                        {
                            var code = new List<Command>()
                            {
                                new Command(){ Name = CommandName.Set}
                            };
                            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                        }
                    }

                }
                else
                {
                    var res = generateByteCode(n.RightExpression, currentSymbolTable);
                    var type = new SymbolType(n.Type);
                    if (res.Result.Equals(type))
                    {
                        currentSymbolTable.RegisterSymbol(new TableElement() { Name = n.Name.Content, Type = type});

                        generateForGetData(n.Name, currentSymbolTable);
                        var code = new List<Command>()
                        {
                            new Command(){ Name = CommandName.Set}
                        };
                        addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                    }
                    else
                        throw new Exception();
                }
            }
        }

        private GenerationResult generateForGetData(Token name, SymbolTable currentSymbolTable)
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
                            var code = new List<Command>()
                            {
                                new Command(){ Name = CommandName.PushStatic, Operands = new int[]{ t.OffsetAddress} }
                            };
                            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                        }
                        else
                        {
                            var code = new List<Command>()
                            {
                                new Command(){ Name = CommandName.Push, Operands = new int[]{ t.OffsetAddress} }
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

        private GenerationResult generationForConst(ConstValueNode node, SymbolTable currentSymbolTable)
        {
            var res = new GenerationResult() { Nothing = false };
            var byteCode = new List<Command>()
                    {
                        new Command()
                        {
                            Name = CommandName.PushConst,
                            Operands = new int[1]{ globalTable.GetConstantAddress(node) }
                        }
                    };
            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], byteCode);
            res.Result = new SymbolType(node);
            return res;
        }
        private void addByteCode(MethodDescription method, List<Command> newByteCode)
        {
            if (method.Code == null)
            {
                method.Code = new Command[0];
            }
            var newCodeArray = new Command[method.Code.Length + newByteCode.Count];
            currentModule.Methods[currentModule.Methods.Length - 1].Code.CopyTo(newCodeArray, 0);
            newByteCode.ToArray().CopyTo(newCodeArray, currentModule.Methods[currentModule.Methods.Length - 1].Code.Length);
            currentModule.Methods[currentModule.Methods.Length - 1].Code = newCodeArray;
        }

    }
}
