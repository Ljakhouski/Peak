using System;
using System.Collections.Generic;
using System.IO;
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
            addBoolConstantsInSymbolTable();
            currentModule = new RuntimeModule();
            currentModule.Methods = new MethodDescription[1] { new MethodDescription() { Code = new Instruction[0] } };
            byteCodePointer.Push(currentModule.Methods[0]); // add reference to "GLOBAL" method
            globalTable.CurrentMethod = currentModule.Methods[0];
            generateForProgramNode(programNode, globalTable);
            writeConstantSection();
            writeGlobalMemoryInfo();
            writeCompletion(globalTable);
            return currentModule;
        }

        private void writeCompletion(SymbolTable globalTable)
        {
            addByteCode(InstructionName.Return, globalTable.CurrentMethod);
        }
        private void addBoolConstantsInSymbolTable()
        {
            globalTable.ConstandData.Add(new Constant { Type = ConstantType.Bool, BoolValue = false });
            globalTable.ConstandData.Add(new Constant { Type = ConstantType.Bool, BoolValue = true });
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
                        addByteCode(
                            generateForAssignment(binary, currentSymbolTable),
                            currentSymbolTable.CurrentMethod
                            );
                    }
                    else
                        throw new Exception();
                }
                else if (n is ProcedureNode)
                {
                    generateMethodDeclaration(n as ProcedureNode, currentSymbolTable);
                }
                else if (n is MethodCallNode)
                {
                    var res = generateMethodCall(n as MethodCallNode, currentSymbolTable, currentSymbolTable);
                    addByteCode(res, currentSymbolTable.CurrentMethod);
                    if (res.Nothing == false)
                        addByteCode(InstructionName.Pop, currentSymbolTable.CurrentMethod);
                }
                else if (n is IfNode)
                {
                    generateIf(n as IfNode, currentSymbolTable);
                }
                else if (n is WhileNode)
                {
                    generateWhile(n as WhileNode, currentSymbolTable);
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
                    case "=":
                    case ">":
                    case "<":
                    case ">=":
                    case "<=":
                    case "!=":
                        return generateComparison(node as BinaryNode, currentSymbolTable);
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

            if (Directory.Exists(Directory.GetCurrentDirectory() + "/" + fileName)
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/" + fileName + ".p")
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/lib/" + fileName)
                ||
                Directory.Exists(Directory.GetCurrentDirectory() + "/lib/" + fileName + ".p")
                ||
                Directory.Exists(node.MetaInf.File + "/" + fileName)
                ||
                Directory.Exists(node.MetaInf.File + "/" + fileName + ".p")
                )
            {
                if (currentSymbolTable.IsNewFile(fileName))
                {
                    currentSymbolTable.RegisterFile(fileName);
                    var p = new Parser();
                    this.generateForProgramNode(p.GetNode(fileName), currentSymbolTable);

                }
                else
                    Error.WarningMessage(node.MetaInf, "file already loadet");
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
                        addByteCode(type, currentSymbolTable.CurrentMethod);
                        var res = generateStoreName(n.Name, currentSymbolTable);
                        addByteCode(res, currentSymbolTable.CurrentMethod);
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

                            res.GeneratedByteCode.AddByteCode(generateStoreName(n.Name, currentSymbolTable));
                            addByteCode(res, currentSymbolTable.CurrentMethod);
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
        }*/

        private void addByteCode(InstructionName instruction, MethodDescription method)
        {
            var code = method.Code;

            if (code == null)
                code = new Instruction[0];

            Array.Resize(ref code, code.Length + 1);
            code[code.Length - 1] = new Instruction() { Name = instruction };
            method.Code = code;
        }

        private void addByteCode(InstructionName instruction, int operand, MethodDescription method)
        {
            var code = method.Code;

            if (code == null)
                code = new Instruction[0];

            Array.Resize(ref code, code.Length + 1);
            code[code.Length - 1] = new Instruction() { Name = instruction, Operands = new int[1] { operand } };
            method.Code = code;
        }

        private void addByteCode(GenerationResult result, MethodDescription method)
        {
            var genCode = result.GeneratedByteCode.ByteCode;
            var code = method.Code;

            if (code == null)
                code = new Instruction[0];

            Array.Resize(ref code, code.Length + genCode.Count);
            genCode.ToArray().CopyTo(code, code.Length - genCode.Count);
            method.Code = code;
        }
    }
}
