using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class Expression
    {
        public static GenResult Generate(Node node, SymbolTable st)
        {
            if (node is ConstValueNode)
            {
                return Constant.Generate(node as ConstValueNode, st);
            }
            else if (node is IdentifierNode)
            {
                return Identifier.Generate(node as IdentifierNode, st);
            }
            else if (node is BinaryNode)
            {
                return BinaryOperator.Generate(node as BinaryNode, st);
            }
            else if (node is MethodCallNode)
            {
                return MethodCall.Generate((node as MethodCallNode), st, st);
            }
            else throw new CompileException();
        }
    }
}
