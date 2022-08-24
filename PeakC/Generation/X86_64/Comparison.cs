using Peak.PeakC;
using Peak.PeakC.Generation;
using Peak.PeakC.Generation.X86_64;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class Comparison
    {
        // take only int/double type
        public static GenResult Generate(BinaryNode node, SymbolTable st, string trueLabel, string falseLabel = "")
        {
            var alloc = st.MemoryAllocator;

            var left = Expression.Generate(node.Left, st);
            var right = Expression.Generate(node.Right, st);

            var generateType = left.ResultType.Type; // TODO: separate int or double (SSE) mode
            checkTypes();

            var left_t = alloc.MoveToAnyRegister(left);
            var left_r = alloc.GetRegister(left_t);
            alloc.Block(left_t);
            var right_t = alloc.MoveToAnyRegister(right);
            var right_r = alloc.GetRegister(right_t);
            alloc.Block(right_r);

            st.Emit($"cmp {left_r}, {right_r}");

            if (node.Operator == ">")
            {
                st.Emit($"jg {trueLabel}");
            }
            else if (node.Operator == ">=")
            {
                st.Emit($"jge {trueLabel}");
            }
            else if (node.Operator == "<")
            {
                st.Emit($"jl {trueLabel}");
            }
            else if (node.Operator == "<=")
            {
                st.Emit($"jle {trueLabel}");
            }
            else if (node.Operator == "=")
            {
                st.Emit($"je {trueLabel}");
            }
            else if (node.Operator == "!=")
            {
                st.Emit($"jne {trueLabel}");
            }
            else
                throw new CompileException();

            if (falseLabel != "")
            {
                st.Emit($"jmp {falseLabel}");
            }

            alloc.Unblock(left_r);
            alloc.Unblock(right_r);

            return new EmptyGenResult();

            void checkTypes()
            {
                if (left.ResultType.Type != Peak.PeakC.Generation.Type.Int
                    ||
                    left.ResultType.Type != Peak.PeakC.Generation.Type.Double)
                    Error.ErrMessage(node.Left.MetaInf, "wrong expression type");

                if (right.ResultType.Type != Peak.PeakC.Generation.Type.Int
                    ||
                    right.ResultType.Type != Peak.PeakC.Generation.Type.Double)
                    Error.ErrMessage(node.Right.MetaInf, "wrong expression type");

                if (left.ResultType != right.ResultType)
                    Error.ErrMessage(node.Right.MetaInf, "types do not match");
            }

        }

        

    }
}
