using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.CodeGeneration
{
    class SymbolType
    {
        public enum Type
        {
            Bool,
            Int,
            Double,
            Str,
            Proc,
            Func,
            Object, // name of struct instance
            Array,
            Dict,
        }
        public Type Value { get; set; }
        public SymbolType SecondValue { get; set; } // only for define type of array/struct/stack/dict...
        public List<SymbolType> Args { get; set; }
        public SymbolType ReturnType { get; set; } // only for procedure

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public SymbolType(ConstValueNode node)
        {
            makeSymbolTypeForConst(node);
        }

        public SymbolType(Node node)
        {
            if (node is ConstValueNode)
                makeSymbolTypeForConst((ConstValueNode)node);
            else if (node is ConstantNode)
                makeSymbolTypeForConstantNode((ConstantNode)node);
        }

        private void makeSymbolTypeForConstantNode(ConstantNode node)
        {
            
        }

        private void makeSymbolTypeForConst(ConstValueNode node)
        {
            if (node.Value.Type == type.IntValue)
            {
                this.Value = Type.Int;
            }
            else if (node.Value.Type == type.DoubleValue)
            {
                this.Value = Type.Double;
            }
            else if (node.Value.Type == type.StrValue)
            {
                this.Value = Type.Str;
            }
            else
                throw new Exception();
        }
    }
}
