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
    }
}
