namespace Peak.PeakC.Generation.X86_64
{
    static class BooleanLogic
    {
        // can take bool-type-expession or comparison-expression (boolean logical and > < >= <= != =)
        // in both cases need to make cast expression to conditional jump ( <bool_expr> == true? jmp trueLabel )
        public static GenResult Generate(BinaryNode node, SymbolTable st, string trueLabel, string falseLabel) 
        {

            if (node.Operator.Content == "and")
            {
                var leftExprIsTrueLabel = IdGenerator.GenerateLabel();
                //TODO: cast boolean expr 
                CastBoolToCompareAndGenerate(node.Left, st, leftExprIsTrueLabel, falseLabel);
                st.Emit($"{leftExprIsTrueLabel}:");
                CastBoolToCompareAndGenerate(node.Right, st, trueLabel, falseLabel);
            }
            else if (node.Operator.Content == "or")
            {
                var rightExprIsFalseLabel = IdGenerator.GenerateLabel();

                CastBoolToCompareAndGenerate(node.Left, st, trueLabel, rightExprIsFalseLabel);
                st.Emit($"{rightExprIsFalseLabel}:");
                CastBoolToCompareAndGenerate(node.Right, st, trueLabel, falseLabel);
            }
            else
                throw new CompileException();

            return new EmptyGenResult();
        }

        public static void CastBoolToCompareAndGenerate(Node node, SymbolTable st, string trueLabel, string falseLabel)
        {
            if (isComparisonOperation(node))
            {
                Comparison.Generate(node as BinaryNode, st, trueLabel, falseLabel);
            }
            else
            {
                var result = Expression.Generate(node, st);
                if (result.ResultType.Type != Peak.PeakC.Generation.Type.Double)
                    Error.ErrMessage(node.MetaInf, "boolean type expected");

                st.MemoryAllocator.MoveToAnyRegister(result);
                var reg = st.MemoryAllocator.GetRegister(result.ReturnDataId);
                st.Emit($"cmp {reg}, 1");
                st.Emit($"je {trueLabel}");
                if (falseLabel != "")
                    st.Emit($"jmp {falseLabel}");
            }
        }
        private static bool isComparisonOperation(Node node)
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
