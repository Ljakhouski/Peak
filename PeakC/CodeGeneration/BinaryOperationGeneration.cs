using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.CodeGeneration
{
    partial class ByteCodeGenerator
    {
        private GenerationResult generateForAssignment(BinaryNode n, SymbolTable currentSymbolTable)
        {
            if (n.Operator != "<<")
                throw new Exception();


            var right = generateByteCode(n.Right, currentSymbolTable);
            var left = generateStoreData(n.Left, currentSymbolTable);

            if (left.ExprResult.Equals(right.ExprResult))
            {
                return new GenerationResult() { Nothing = true };
            }
            else
                Error.ErrMessage(n.Operator,
                    "assignment " + left.ExprResult.ToString()
                    + " and " + right.ExprResult.ToString()
                    + " not possible");
            throw new Exception();
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
                (left.ExprResult.Value == SymbolType.Type.Int
             || left.ExprResult.Value == SymbolType.Type.Double)
             &&
             (right.ExprResult.Value == SymbolType.Type.Int
             || right.ExprResult.Value == SymbolType.Type.Double))
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
               left.ExprResult.Value == SymbolType.Type.Str)
            {
                throw new Exception("temporarily unsupported");
            }
            else
                Error.ErrMessage(n.Operator,
                    "operator \"" + n.Operator + "\" does not accept type " + left.ExprResult.ToString()
                    + " and " + right.ExprResult.ToString());
            throw new Exception();
        }

        private GenerationResult generateComparison(BinaryNode n, SymbolTable currentSymbolTable)
        {
            var left = generateByteCode(n.Left, currentSymbolTable);
            var right = generateByteCode(n.Right, currentSymbolTable);

            if (left.Nothing)
                Error.ErrMessage(n.Left.MetaInf, "expression must be return value");

            else if (right.Nothing)
                Error.ErrMessage(n.Right.MetaInf, "expression must be return value");

            var res = new GenerationResult() { Nothing = false, ExprResult = new SymbolType(SymbolType.Type.Bool) };
            res.GeneratedByteCode.AddByteCode(left);
            res.GeneratedByteCode.AddByteCode(right);

            if (n.Operator == "=")
            {
                if (left.ExprResult == right.ExprResult)
                {
                    switch (left.ExprResult.Value)
                    {
                        case SymbolType.Type.Bool:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsBool);
                            break;
                        case SymbolType.Type.Int:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsInt);
                            break;
                        case SymbolType.Type.Double:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsDouble);
                            break;
                        case SymbolType.Type.Str:
                            res.GeneratedByteCode.AddByteCode(InstructionName.EqualsString);
                            break;
                        default:
                            Error.ErrMessage(null, "");
                            break;
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

                if (left.ExprResult.Value == SymbolType.Type.Int)
                {
                    res.GeneratedByteCode.AddByteCode(InstructionName.MoreInt);
                }
                else if (left.ExprResult.Value == SymbolType.Type.Double)
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

                



            }
            else if (true)
            {
                if (left.ExprResult == right.ExprResult)
                {
                    if (left.ExprResult.Value == SymbolType.Type.Int
                        ||
                        left.ExprResult.Value == SymbolType.Type.Double)
                    {
                        //res.GeneratedByteCode.AddByteCode(InstructionName.Less);
                        return res;
                    }
                    else
                        Error.ErrMessage(n.Operator, "expressions must be of type int or double");
                }
                else
                    Error.ErrMessage(n.Operator, "expressions should be the same");
            }
            else
                throw new CompileException();
            throw new CompileException();
        }
    }
}
