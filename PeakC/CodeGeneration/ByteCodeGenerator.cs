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
        private RuntimeModule currentModule;
        private SymbolTable globalTable;
        private Stack<MethodDescription> byteCodePointer = new Stack<MethodDescription>();
        public ByteCodeGenerator()
        {

        }

        public RuntimeModule GetProgramRuntimeModule(ProgramNode programNode)
        {
            globalTable = new SymbolTable() { IsGlobalScope = true };
            currentModule = new RuntimeModule();
            currentModule.Methods = new MethodDescription[1] { new MethodDescription() };
            byteCodePointer.Push(currentModule.Methods[0]); // add reference to "GLOBAL" method
            globalTable.CurrentMethod = currentModule.Methods[0];
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
            currentModule.Methods[0].Name = "GLOBAL";
        }
        private void generateForProgramNode(ProgramNode node, SymbolTable currentSymbolTable)
        {
            foreach (Node n in node.Node)
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
                    generateForProgramNode((ProgramNode)n, currentSymbolTable);
                }
                else if (n is BinaryNode)
                {
                    var binary = (BinaryNode)n;

                    if (binary.Operator == "<<")
                    {
                        var res = generateForAssignment(binary, currentSymbolTable);
                        // TODO: make it
                        //var method = currentSymbolTable.CurrentMethod.
                    }
                    else
                        throw new Exception();
                }
                else if (n is ProcedureNode)
                {
                    generateProcDeclaration(n as ProcedureNode, currentSymbolTable);
                }
                else if (n is MethodCallNode)
                {
                    generateMethodCall(n as MethodCallNode, currentSymbolTable, currentSymbolTable);
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
            else if (node is IdentifierNode)
            {
                return generatePushOnStackName((node as IdentifierNode).Id, currentSymbolTable);
            }
            else if (node is BinaryNode)
            {
                switch (((BinaryNode)node).Operator.Content)
                {
                    case "+":
                    case "-":
                    case "*":
                    case "/":
                        return generateForMathOperators(node as BinaryNode, currentSymbolTable);
                        break;
                    default:
                        throw new Exception("does not supported");
                }
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
            if (currentSymbolTable.ContainsInAllTables(n.Name))
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
                        currentSymbolTable.RegisterSymbol(new TableElement() { Type = type.ExprResult, InfoNode = n, Name = n.Name.Content });
                        var res = generateStoreName(n.Name, currentSymbolTable);

                    }
                }
                else
                {
                    var type = new SymbolType(n.Type);

                    if (n.RightExpression != null)
                    {
                        var res = generateByteCode(n.RightExpression, currentSymbolTable);

                        if (res.ExprResult.Equals(type))
                        {
                            currentSymbolTable.RegisterSymbol(new TableElement() { Name = n.Name.Content, Type = type });

                            generateStoreName(n.Name, currentSymbolTable);
                        }
                        else
                            Error.ErrMessage(n.MetaInf, "variable type does not match");
                    }
                    else
                    {
                        currentSymbolTable.RegisterSymbol(new TableElement() { Name = n.Name.Content, Type = type });
                    }
                }
            }
        }



        private GenerationResult generationForConst(ConstValueNode node, SymbolTable currentSymbolTable)
        {
            var res = new GenerationResult() { Nothing = false };

            res.GeneratedByteCode.AddByteCode(InstructionName.PushConst, globalTable.GetConstantAddress(node));
            res.ExprResult = new SymbolType(node);
            return res;
        }
        /*
        [ObsoleteAttribute("This method is obsolete")]
        private void addByteCode(MethodDescription method, List<Instruction> newByteCode)
        {
            if (method.Code == null)
            {
                method.Code = new Instruction[0];
            }
            var newCodeArray = new Instruction[method.Code.Length + newByteCode.Count];
            currentModule.Methods[currentModule.Methods.Length - 1].Code.CopyTo(newCodeArray, 0);
            newByteCode.ToArray().CopyTo(newCodeArray, currentModule.Methods[currentModule.Methods.Length - 1].Code.Length);
            currentModule.Methods[currentModule.Methods.Length - 1].Code = newCodeArray;
        }

        private void addByteCode(MethodDescription method, InstructionName name, int[] operands = null)
        {
            var command = new Instruction() { Name = name, Operands = operands };
            if (method.Code == null)
            {
                method.Code = new Instruction[1] { command };
            }
            else
            {
                var code = method.Code;
                Array.Resize(ref code, code.Length + 1);
                code[code.Length - 1] = command;
                method.Code = code;
            }
        }
      
        private void addByteCode(InstructionName name, int operand)
        {
            addByteCode(byteCodePointer.Peek(), name, new int[] { operand });
        }

        private void addByteCode(InstructionName name, int op1, int op2)
        {
            addByteCode(byteCodePointer.Peek(), name, new int[] { op1, op2 });
        }

        private void addByteCode(InstructionName name)
        {
            addByteCode(byteCodePointer.Peek(), name);
        }*/

    }
}
