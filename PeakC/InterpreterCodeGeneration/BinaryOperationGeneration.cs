﻿using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.PeakC.Generation.InterpreterCodeGeneration
{
    partial class ByteCodeGenerator
    {
        private GenerationResult generateForAssignment(BinaryNode n, SymbolTable currentSymbolTable)
        {
            if (n.Operator != "<<")
                throw new CompileException();


            var right = generateByteCode(n.Right, currentSymbolTable);
            var left = generateStoreData(n.Left, currentSymbolTable);

            if (left.Nothing == false && left.ExprResult.Equals(right.ExprResult))
            {
                var res = new GenerationResult() { Nothing = true };
                res.GeneratedByteCode.AddByteCode(right);
                res.GeneratedByteCode.AddByteCode(left);
                return res;
            }
            else
                Error.ErrMessage(n.Operator,
                    "assignment " + left.ExprResult.ToString()
                    + " and " + right.ExprResult.ToString()
                    + " not possible");
            throw new CompileException();
        }

        private GenerationResult generateForMathOperators(BinaryNode n, SymbolTable currentSymbolTable)
        {
            var left = generateByteCode(n.Left, currentSymbolTable);
            var right = generateByteCode(n.Right, currentSymbolTable);

            if (left.Nothing)
                Error.ErrMessage(n.Left.MetaInf, "expression must be return \"int\" or \"double\"");

            else if (right.Nothing)
                Error.ErrMessage(n.Right.MetaInf, "expression must be return \"int\" or \"double\"");


            if (/*left.Result == right.Result &&*/
                (left.ExprResult.Value == SemanticType.Type.Int
             || left.ExprResult.Value == SemanticType.Type.Double)
             &&
             (right.ExprResult.Value == SemanticType.Type.Int
             || right.ExprResult.Value == SemanticType.Type.Double))
            {

                var result = new GenerationResult() { Nothing = false, ExprResult = left.ExprResult };

                result.GeneratedByteCode.AddByteCode(left);
                result.GeneratedByteCode.AddByteCode(right);

                switch (n.Operator.Content)
                {
                    case "+":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Add);
                        break;
                    case "-":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Sub);
                        break;
                    case "*":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Mul);
                        break;
                    case "/":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Div);
                        break;
                }

                return result;
            }
            else if (left.ExprResult == right.ExprResult &&
               left.ExprResult.Value == SemanticType.Type.Str)
            {
                throw new CompileException("temporarily unsupported");
            }
            else
                Error.ErrMessage(n.Operator,
                    "operator \"" + n.Operator + "\" does not accept type " + left.ExprResult.ToString()
                    + " and " + right.ExprResult.ToString());
            throw new CompileException();
        }

        private GenerationResult generateComparison(BinaryNode n, SymbolTable currentSymbolTable)
        {
            var left = generateByteCode(n.Left, currentSymbolTable);
            var right = generateByteCode(n.Right, currentSymbolTable);

            if (left.Nothing)
                Error.ErrMessage(n.Left.MetaInf, "expression must be return value");

            else if (right.Nothing)
                Error.ErrMessage(n.Right.MetaInf, "expression must be return value");

            var res = new GenerationResult() { Nothing = false, ExprResult = new SemanticType(SemanticType.Type.Bool) };
            res.GeneratedByteCode.AddByteCode(left);
            res.GeneratedByteCode.AddByteCode(right);

            if (n.Operator == "=" || n.Operator == "!=")
            {
                if (left.ExprResult == right.ExprResult)
                {
                    switch (left.ExprResult.Value)
                    {
                        case SemanticType.Type.Bool:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsBool);
                            break;
                        case SemanticType.Type.Int:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsInt);
                            break;
                        case SemanticType.Type.Double:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsDouble);
                            break;
                        case SemanticType.Type.Str:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsString);
                            break;
                        default:
                            Error.ErrMessage(n.Operator, "incomparable types");
                            break;
                    }
                    if (n.Operator == "!=")
                    {
                        /*    INVERSION    */
                        res.GeneratedByteCode.AddByteCode(InstructionName.PushConst, 0);
                        res.GeneratedByteCode.AddByteCode(InstructionName.EqualsBool);
                    }
                    return res;
                }
                else
                    Error.ErrMessage(n.Operator, "expressions should be the same");

            }
            else if (n.Operator == ">" || n.Operator == "<" || n.Operator == ">=" || n.Operator == "=<")
            {
                if (left.ExprResult != right.ExprResult)
                    Error.ErrMessage(n.Operator, "expressions should be the same");

                // copy two comparison values to equals defore '<' or '>'

                if (n.Operator == ">=" || n.Operator == "<=")
                {
                    res.GeneratedByteCode.AddByteCode(InstructionName.PushCopy, 2);
                    res.GeneratedByteCode.AddByteCode(InstructionName.PushCopy, 2);
                }

                if (left.ExprResult.Value == SemanticType.Type.Int)
                {
                    res.GeneratedByteCode.AddByteCode(InstructionName.MoreInt);
                }
                else if (left.ExprResult.Value == SemanticType.Type.Double)
                {
                    res.GeneratedByteCode.AddByteCode(InstructionName.MoreDouble);
                }
                else
                    Error.ErrMessage(n.Operator, "expressions must be of type int or double");

                if (n.Operator == "<" || n.Operator == "<=")
                {
                    /*    INVERSION    */
                    res.GeneratedByteCode.AddByteCode(InstructionName.PushConst, 0);
                    res.GeneratedByteCode.AddByteCode(InstructionName.EqualsBool);
                }

                if (n.Operator == ">=" || n.Operator == "<=")
                {
                    res.GeneratedByteCode.AddByteCode(InstructionName.PushCopy, 2);
                    res.GeneratedByteCode.AddByteCode(InstructionName.PushCopy, 2);
                    res.GeneratedByteCode.AddByteCode(InstructionName.EqualsBool);

                    // on stack bool,bool. Compare "OR" now
                    res.GeneratedByteCode.AddByteCode(InstructionName.OrEqualsBool);
                    res.GeneratedByteCode.AddByteCode(InstructionName.PopCopy, 3);
                    res.GeneratedByteCode.AddByteCode(InstructionName.Pop, 3);
                }

                return res;

            }
            else if (n.Operator == "and" || n.Operator == "or")
            {
                if (left.ExprResult == right.ExprResult && left.ExprResult.Value == SemanticType.Type.Bool)
                    Error.ErrMessage(n.Operator, "expressions should be the bool types");

                if (n.Operator == "and")
                    res.GeneratedByteCode.AddByteCode(InstructionName.EqualsBool);
                else
                    res.GeneratedByteCode.AddByteCode(InstructionName.OrEqualsBool);

                return res;
            }
            throw new CompileException();
        }
    }
}
