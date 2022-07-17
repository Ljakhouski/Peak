using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class Constant
    {
        public static GenResult Generate(ConstValueNode node, SymbolTable st)
        {
            return new ConstantResult()
            {
                /* = int.Parse(node.Value.Content),
                DoubleValue = double.Parse(node.Value.Content),
                BoolValue = bool.Parse(node.Value.Content),*/
                ResultType = new SymbolType(node),
                ReturnDataId = null
            };
        }
    }
}
