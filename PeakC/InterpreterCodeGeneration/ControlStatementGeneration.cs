using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.PeakC.Generation.InterpreterCodeGeneration
{
    partial class ByteCodeGenerator
    {
        private void generateIf(IfNode n, SymbolTable currentSymbolTable)
        {

            int getCurrentPosition()
            {
                if (currentSymbolTable.CurrentMethod.Code.Length == 0)
                    return 0;
                return currentSymbolTable.CurrentMethod.Code.Length - 1;
            }
            void setOperand(int positionPointer, int operand)
            {
                currentSymbolTable.CurrentMethod.Code[positionPointer].Operands = new int[1] { operand };
            }

            var condition = generateByteCode(n.Condition, currentSymbolTable);

            if (condition.ExprResult.Value == SemanticType.Type.Bool)
            {
                addByteCode(condition, currentSymbolTable.CurrentMethod);
                addByteCode(InstructionName.IfNot, currentSymbolTable.CurrentMethod);

                var ifNotPointer = getCurrentPosition();

                generateForCodeBlock(n.IfTrueCode, new SymbolTable()
                {
                    Prev = currentSymbolTable,
                    IsGlobalScope = currentSymbolTable.IsGlobalScope,
                    IsMethodDefTable = false,
                    CurrentMethod = currentSymbolTable.CurrentMethod
                });

                //addByteCode(InstructionName.Jump, currentSymbolTable.CurrentMethod);
                //var endOfIfTruePointer = getCurrentPosition();

                if (n.ElseCode is null)
                {
                    int position = getCurrentPosition() + 1;
                    //currentSymbolTable.CurrentMethod.Code[ifNotPointer].Operands[0] = position;
                    setOperand(ifNotPointer, position);
                    return;
                }
                else
                {
                    addByteCode(InstructionName.Jump, currentSymbolTable.CurrentMethod);
                    int endOfIfTruePointer = getCurrentPosition(); // position of the jump-command for jump after if-true-execute to the end of if[]else[] execute

                    //currentSymbolTable.CurrentMethod.Code[ifNotPointer].Operands[0] = getCurrentPosition() + 1;
                    setOperand(ifNotPointer, getCurrentPosition() + 1);
                    generateForCodeBlock(n.ElseCode, new SymbolTable()
                    {
                        Prev = currentSymbolTable,
                        IsGlobalScope = currentSymbolTable.IsGlobalScope,
                        IsMethodDefTable = false,
                        CurrentMethod = currentSymbolTable.CurrentMethod
                    });
                    int endElsePosition = getCurrentPosition() + 1;
                    //currentSymbolTable.CurrentMethod.Code[endOfIfTruePointer].Operands[0] = endElsePosition;
                    setOperand(endOfIfTruePointer, endElsePosition);
                    return;
                }
            }
            else
                Error.ErrMessage(n.Condition.MetaInf, "boolean expression expected");
            throw new CompileException();
        }

        private void generateWhile(WhileNode n, SymbolTable currentSymbolTable)
        {
            int getCurrentPosition()
            {
                if (currentSymbolTable.CurrentMethod.Code.Length == 0)
                    return 0;
                return currentSymbolTable.CurrentMethod.Code.Length - 1;
            }
            void setOperand(int positionPointer, int operand)
            {
                currentSymbolTable.CurrentMethod.Code[positionPointer].Operands = new int[1] { operand };
            }

            var condition = generateByteCode(n.Condition, currentSymbolTable);

            if (condition.ExprResult.Value == SemanticType.Type.Bool)
            {
                int nextInstruction = 0;
                if (getCurrentPosition() > 0)
                    nextInstruction = 1;

                var cyclePointer = getCurrentPosition() + nextInstruction;

                addByteCode(condition, currentSymbolTable.CurrentMethod);

                currentSymbolTable.StartOfCycleAddress = cyclePointer;
                addByteCode(InstructionName.IfNot, currentSymbolTable.CurrentMethod);
                var ifNotPointer = getCurrentPosition();
                generateForCodeBlock(n.Code, new SymbolTable()
                {
                    Prev = currentSymbolTable,
                    IsGlobalScope = currentSymbolTable.IsGlobalScope,
                    IsMethodDefTable = false,
                    CurrentMethod = currentSymbolTable.CurrentMethod
                });

                // jump to cycle pointer
                addByteCode(InstructionName.Jump, currentSymbolTable.CurrentMethod);
                setOperand(getCurrentPosition(), cyclePointer);

                // set break;
                var breakFromCyclePointer = getCurrentPosition() + 1;
                currentSymbolTable.EndOfCycleAddress = breakFromCyclePointer;
                setOperand(ifNotPointer, breakFromCyclePointer);
                return;
            }
            else
                Error.ErrMessage(n.Condition.MetaInf, "boolean expression expected");
            throw new CompileException();
        }

        private void generateWordOperator(WordOperatorNode node, SymbolTable currentSymbolTable)
        {
            switch (node.Operator)
            {
                case "return":
                    addByteCode(generateReturn(node, currentSymbolTable), currentSymbolTable.CurrentMethod);
                    break;
                case "break":
                    addByteCode(generateBreak(node, currentSymbolTable), currentSymbolTable.CurrentMethod);
                    break;
                default:
                    throw new CompileException();
            }
        }

        private GenerationResult generateReturn(WordOperatorNode node, SymbolTable currentSymbolTable)
        {
            var retResult = generateByteCode(node.Expression, currentSymbolTable);

            retResult.GeneratedByteCode.AddByteCode(InstructionName.Return);

            var table = currentSymbolTable;

            if (table.IsMethodDefTable)
            {
                var type = table.MethodElement.Type.ReturnType;
                if (type == retResult.ExprResult)
                    return retResult;
                else
                    Error.ErrMessage(node.Expression.MetaInf, "wrong expression type for \"return\"");
            }

            while (table.Prev is null == false)
            {
                table = table.Prev;

                if (table.IsMethodDefTable && table.IsGlobalScope == false)
                {
                    var type = table.MethodElement.Type.ReturnType;
                    if (type == retResult.ExprResult)
                        return retResult;
                    else
                        Error.ErrMessage(node.Expression.MetaInf, "wrong expression type for \"return\"");
                }
                else if (table.IsStructDefTable)
                    Error.ErrMessage(node.MetaInf, "missing function or procedure to return");
                else
                    continue;
            }
            Error.ErrMessage(node.MetaInf, "missing function or procedure to return");
            //var result = new GenerationResult() { }
            return null;
        }
        private GenerationResult generateBreak (WordOperatorNode node, SymbolTable currentSymbolTable)
        {
            var res = new GenerationResult() { Nothing = true };

            var table = currentSymbolTable;
            if (table.IsCycleDefTable)
            {
                res.GeneratedByteCode.AddByteCode(InstructionName.Jump, table.EndOfCycleAddress);
                return res;
            }

            while (table.Prev is null == false)
            {
                table = table.Prev;

                if (table.IsCycleDefTable == false)
                    continue;
                else if (table.IsMethodDefTable || table.IsStructDefTable)
                    Error.ErrMessage(node.MetaInf, "missing cycle");
                else
                {
                    res.GeneratedByteCode.AddByteCode(InstructionName.Jump, table.EndOfCycleAddress);
                    return res;
                }
            }
            Error.ErrMessage(node.MetaInf, "missing cycle");
            return null;
        }
    }
}
