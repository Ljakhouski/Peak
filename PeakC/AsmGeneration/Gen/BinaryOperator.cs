using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class BinaryOperator
    {
        public static GenResult Generate(BinaryNode node, SymbolTable st)
        {
            var right = node.Right;
            var left = node.Left;

            if (node.Operator.Content == "<-")
            {
                Assignment.Generate(right, left, st);
                return new EmptyGenResult();
            }
            else throw new CompileException("non implemented operator ");
            /*var rightResult = Expression.Generate(right, st);
            
            if (left is IdentifierNode)
            {

            }*/

            // else if (left is GetArrayItemNode)
            // else if (left is DotNode)
        }
    }
}
