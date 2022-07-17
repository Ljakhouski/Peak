using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
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
            else throw new CompileException();
        }
    }
}
