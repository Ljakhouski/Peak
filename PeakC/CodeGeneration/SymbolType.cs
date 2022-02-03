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
    }
}
