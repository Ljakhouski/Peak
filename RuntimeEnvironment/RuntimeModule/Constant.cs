using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    public enum ConstantType
    {
        Bool,
        Int,
        Double,
        Str,
    }
    [Serializable]
    public class Constant
    {
        public ConstantType Type { get; set; }

        public bool BoolValue { get; set; }
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public string StrValue { get; set; }

    }
}
