using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
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

            if (left.ResultType != right.ResultType)
                Error.ErrMessage(op, "expression types do not match");

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
                }

                result.ConstValue = new Token() { Content = result_d.ToString(), File = op.File, IsEmpty = false, Line = op.Line, Position = op.Position/*, Type = type.IntValue*/};
            }
            else
                Error.ErrMessage(op, "math operation expected");

            return result;
        }


        private static GenResult makeCalculation(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            if (leftRes.ResultType != leftRes.ResultType)
                Error.ErrMessage(op, "expression types do not match");

            bool isSSE = false;

            if (isSSE == false)
                switch (op.Content)
                {
                    case "+":
                    case "-":
                        return emitPlusOrMinus(leftRes, rightRes, op, st);
                    case "*":
                        return emitMul(leftRes, rightRes, st);
                    case "/":
                        return emitDiv(leftRes, rightRes, op, st);
                    default:
                        throw new CompileException();
                }
            else
                throw new CompileException();
        }

        private static GenResult emitPlusOrMinus(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            var left = st.MemoryAllocator.MoveToAnyRegister(leftRes);
            st.MemoryAllocator.Block(left);

            var op1 = left.Register;
            var op2 = "";

            // second operand can be as constant
            if (rightRes is ConstantResult)
                op2 = toConstOperand(rightRes);
            else
                op2 = st.MemoryAllocator.MoveToAnyRegister(rightRes).Register.ToString();


            if      (op.Content == "+") st.Emit($"add {op1}, {op2}");
            else if (op.Content == "-") st.Emit($"sub {op1}, {op2}");

            st.MemoryAllocator.Unblock(left);
            st.MemoryAllocator.SetRegisterFree(rightRes);

            var result = new GenResult()
            {
                ResultType = new SemanticType(Type.Int),
                ReturnDataId = new MemoryIdTracker(st, 8)
            };

            st.MemoryAllocator.SetRegister(result.ReturnDataId, op1);

            return result;
        }

        private static GenResult emitMul(GenResult leftRes, GenResult rightRes, SymbolTable st)
        {
            var left = st.MemoryAllocator.MoveToAnyRegister(leftRes);
            st.MemoryAllocator.Block(left);

            var op1 = left.Register;
            var op2 = "";

            // second operand can be as constant
            if (rightRes is ConstantResult)
                op2 = toConstOperand(rightRes);
            else
                op2 = st.MemoryAllocator.MoveToAnyRegister(rightRes).Register.ToString();

            st.Emit($"imul {op1}, {op2}");

            st.MemoryAllocator.Unblock(left);
            st.MemoryAllocator.SetRegisterFree(rightRes);

            var result = new GenResult()
            {
                ResultType = new SemanticType(Type.Int),
                ReturnDataId = new MemoryIdTracker(st, 8)
            };

            st.MemoryAllocator.SetRegisterFree(op1);
            st.MemoryAllocator.SetIdToFreeRegister(result.ReturnDataId, op1);


            return result;
        }

        private static GenResult emitDiv(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            if (rightRes is ConstantResult && isZero(rightRes as ConstantResult))
                Error.ErrMessage(op, "division by zero");

            // free rdx and clean
            // mov first operand to rax
            // second operand in op2 (divOp)
            // result placed in rax

            st.MemoryAllocator.MoveToRegister(leftRes, RegisterName.rax);
            st.MemoryAllocator.Block(RegisterName.rax);

            var rightId = st.MemoryAllocator.MoveToAnyRegister(rightRes);
            st.MemoryAllocator.Block(rightId);

            
            st.MemoryAllocator.FreeRegister(RegisterName.rdx);
            st.MemoryAllocator.Block(RegisterName.rdx);
            st.Emit("xor rdx, rdx");

            st.Emit($"idiv {rightId.Register}");

            var result = new GenResult()
            {
                ResultType = new SemanticType(Type.Int),
                ReturnDataId = new MemoryIdTracker(st, 8)
            };

            result.ReturnDataId = new MemoryIdTracker(st, 8);

            // devision result will be in rax

            st.MemoryAllocator.Unblock(rightId);
            st.MemoryAllocator.SetRegisterFree(RegisterName.rax);
            st.MemoryAllocator.SetIdToFreeRegister(result.ReturnDataId, RegisterName.rax);

            st.MemoryAllocator.SetRegisterFree(RegisterName.rdx);
            st.MemoryAllocator.SetRegisterFree(rightId);

            st.MemoryAllocator.Unblock(RegisterName.rdx); // TODO: use rdx to get .00{N} part 
            st.MemoryAllocator.Unblock(RegisterName.rax);

            return result;
        }

        private static GenResult emitPlusOrMinus_SSE(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            throw new NotImplementedException();
        }

        private static GenResult emitMul_SSE(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            throw new NotImplementedException();
        }
        private static GenResult emitDiv_SSE(GenResult leftRes, GenResult rightRes, Token op, SymbolTable st)
        {
            throw new NotImplementedException();
        }


        private static string toConstOperand(GenResult res)
        {
            var r = res as ConstantResult;

            switch (r.ResultType.Type)
            {
                case Type.Int:
                    return r.IntValue;
                case Type.Double:
                    return r.DoubleValue;
                default:
                    //Error.ErrMessage(/*TODO: place meta-token*/"");
                    throw new CompileException();
            }
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
