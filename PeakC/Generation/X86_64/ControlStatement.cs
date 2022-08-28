using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class ControlStatement
    {
        public static GenResult GenerateReturn(WordOperatorNode node, SymbolTable st)
        {
            if (st.MethodTable is GlobalSymbolTable)
                Error.ErrMessage(node.MetaInf, "\"return\" operator not works in global scope");

            var expr = Expression.Generate(node.Expression, st);

            var mst = st.MethodTable;
            var methodSignature = mst.Signature;

            if (methodSignature.RetType is null ||
                expr.ResultType is null ||
                methodSignature.RetType != expr.ResultType)
                Error.ErrMessage(node.MetaInf, "wrong type for \"return\"");

            if (expr.ResultType.Type == Type.Double)
                st.MemoryAllocator.MoveToRegister(expr, RegisterName.xmm0);
            else
                st.MemoryAllocator.MoveToRegister(expr, RegisterName.rax);

            MethodDeclaration.EmitEpilogue(st);
            return new EmptyGenResult();
        }

        public static GenResult GenerateIfElse(IfNode node, SymbolTable st)
        {
            var ifBeginLabel = IdGenerator.GenerateLabel();
            var ifEndLabel = IdGenerator.GenerateLabel();
            var ifElseEndLabel = IdGenerator.GenerateLabel();

            if (isCompareOperator(node.Condition))
            {
                Comparison.Generate(node.Condition as BinaryNode, st, ifBeginLabel, ifEndLabel);
            }
            else
            {
                BooleanLogic.CastBoolToCompareAndGenerate(node.Condition, st, ifBeginLabel, ifEndLabel);
            }

            st.Emit($"{ifBeginLabel}:");

            var ifBody = new SymbolTable()
            {
                Prev = st
            };

            CodeBlock.Generate(node.IfTrueCode, ifBody);

            
            if (node.ElseCode is null == false)
            {
                st.Emit($"jmp {ifElseEndLabel}");
            }

            st.Emit($"{ifEndLabel}:");

            if (node.ElseCode is null == false)
            {
                var elseBody = new SymbolTable()
                {
                    Prev = st
                };

                CodeBlock.Generate(node.ElseCode, elseBody);
                st.Emit($"{ifElseEndLabel}:");
            }

            return new EmptyGenResult();
        }
        public static GenResult GenerateWhile(WhileNode node, SymbolTable st)
        {
            var whileBeginLabel = IdGenerator.GenerateLabel();
            var blockBeginLabel = IdGenerator.GenerateLabel();
            var blockEndLabel = IdGenerator.GenerateLabel();

            st.Emit($"{whileBeginLabel}:");

            //if (isCompareOperator(node.Condition))
            //{
            //    Comparison.Generate(node.Condition as BinaryNode, st, blockBeginLabel, blockEndLabel);
            //}
            //else
            //{
                BooleanLogic.CastBoolToCompareAndGenerate(node.Condition, st, blockBeginLabel, blockEndLabel);
            //}
            st.Emit($"{blockBeginLabel}:");
            var body = new WhileSymbolTable()
            {
                Prev = st,
                WhileBeginLabel = whileBeginLabel,
                WhileEndLabel = blockEndLabel
            };

            CodeBlock.Generate(node.Code, body);
            st.Emit($"jmp {whileBeginLabel}");
            st.Emit($"{blockEndLabel}:");

            return new EmptyGenResult();
        }
        private static bool isCompareOperator(Node node)
        {
            if (node is BinaryNode)
            {
                var bn = node as BinaryNode;
                if (bn.Operator == ">" ||
                    bn.Operator == ">=" ||
                    bn.Operator == "<" ||
                    bn.Operator == "<=" ||
                    bn.Operator == "!=" ||
                    bn.Operator == "=" 
                    )
                    return true;
            }
            return false;
        }
    }
}
