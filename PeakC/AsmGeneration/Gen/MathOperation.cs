using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class MathOperation
    {
        public static GenResult Generate(BinaryNode node, SymbolTable st)
        {
            var left = Expression.Generate(node.Left, st);
            var right = Expression.Generate(node.Right, st);

            if (left is ConstantResult && right is ConstantResult)
            {
                return makeConstantCalculation(left as ConstantResult, right as ConstantResult, node.Operator);
            }
            else
            {
                return makeCalculation(left, right, node.Operator, st);
            }
        }

        private static ConstantResult makeConstantCalculation(ConstantResult left, ConstantResult right, Token op)
        {
            var result = new ConstantResult();

            if (left.ResultType == right.ResultType)
            {
                result.ResultType = left.ResultType;

                if (left.ResultType.Type == Type.Int)
                {
                    var left_i = Int32.Parse(left.IntValue);
                    var right_i = Int32.Parse(right.IntValue);
                    var result_i = 0;
                    switch (op.Content)
                    {
                        case "+":
                            result_i = left_i + right_i;
                            break;
                        case "-":
                            result_i = left_i - right_i;
                            break;
                        case "*":
                            result_i = left_i * right_i;
                            break;
                        case "/":
                            if (right_i == 0)
                                Error.ErrMessage(op, "division by zero");

                            result_i = left_i / right_i;
                            break;
                        default:
                            Error.ErrMessage(op, "it is not operation");
                            throw new CompileException();
                            break;
                    }

                    result.ConstValue = new Token() { Content = result_i.ToString(), File = op.File, IsEmpty = false, Line = op.Line, Position = op.Position/*, Type = type.IntValue*/};
                }
                else if (left.ResultType.Type == Type.Double)
                {
                    var left_d = Double.Parse(left.IntValue);
                    var right_d = Double.Parse(right.IntValue);
                    double result_d = 0;
                    switch (op.Content)
                    {
                        case "+":
                            result_d = left_d + right_d;
                            break;
                        case "-":
                            result_d = left_d - right_d;
                            break;
                        case "*":
                            result_d = left_d * right_d;
                            break;
                        case "/":
                            if (right_d == 0)
                                Error.ErrMessage(op, "division by zero");

                            result_d = left_d / right_d;
                            break;
                        default:
                            Error.ErrMessage(op, "it is not operation");
                            throw new CompileException();
                            break;
                    }

                    result.ConstValue = new Token() { Content = result_d.ToString(), File = op.File, IsEmpty = false, Line = op.Line, Position = op.Position/*, Type = type.IntValue*/};
                }
                else
                    Error.ErrMessage(op, "math operation expected");
            }
            else
                Error.ErrMessage(op, "expression types do not match");
            throw new CompileException();
        }


        private static GenResult makeCalculation(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            if (leftRes.ResultType != leftRes.ResultType)
                Error.ErrMessage(op, "expression types do not match");

            var result = new GenResult();

            var left  = st.MemoryAllocator.MoveToAnyRegister(leftRes);
            var right = st.MemoryAllocator.MoveToAnyRegister(rightRes);

            var reg1 = left.Register;
            var reg2 = right.Register;

            if (leftRes.ResultType.Type == Type.Int)
            {
                switch (op.Content)
                {
                    case "+":
                        st.Emit($"add {reg1}, {reg2}");
                        break;
                    case "-":
                        st.Emit($"sub {reg1}, {reg2}");
                        break;
                    case "*":
                        st.Emit($"mul {reg1}, {reg2}");
                        break;
                    case "/":
                        if (rightRes is ConstantResult && isZero(rightRes as ConstantResult))
                            Error.ErrMessage(op, "division by zero");
                        st.Emit($"div {reg1}, {reg2}");
                        break;
                }

                result.ResultType.Type = Type.Int;
            }

            else if (leftRes.ResultType.Type == Type.Double)
            {
                switch (op.Content)
                {
                    case "+":
                        st.Emit($"addsd {reg1}, {reg2}");
                        break;
                    case "-":
                        st.Emit($"subsd {reg1}, {reg2}");
                        break;
                    case "*":
                        st.Emit($"mulsd {reg1}, {reg2}");
                        break;
                    case "/":
                        if (rightRes is ConstantResult && isZero(rightRes as ConstantResult))
                            Error.ErrMessage(op, "division by zero");
                        st.Emit($"divsd {reg1}, {reg2}");
                        break;
                }

                result.ResultType.Type = Type.Double;
            }

            result.ReturnDataId = leftRes.ReturnDataId;
            return result;

        }

        private static bool isZero(ConstantResult res)
        {
            if (res.ResultType.Type == Type.Int)
            {
                return Int32.Parse(res.ConstValue.Content) == 0;
            }
            else if (res.ResultType.Type == Type.Double)
            {
                return Double.Parse(res.ConstValue.Content) == 0;
            }
            else
                throw new CompileException();
        }
    }
}
