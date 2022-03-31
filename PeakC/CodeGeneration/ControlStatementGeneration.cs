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
        private void generateIf(IfNode n, SymbolTable currentSymbolTable)
        {

            int getCurrentPosition()
            {
                return currentSymbolTable.CurrentMethod.Code.Length - 1;
            }
            void setOperand(int positionPointer, int operand)
            {
                currentSymbolTable.CurrentMethod.Code[positionPointer].Operands = new int[1] { operand };
            }

            var condition = generateByteCode(n.Condition, currentSymbolTable);

            if (condition.ExprResult.Value == SymbolType.Type.Bool)
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
    }
}
