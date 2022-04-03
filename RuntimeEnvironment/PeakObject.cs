using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment
{
    enum PeakObjectType // only for reflection inside runtime
    {
        Int,
        Double,
        Bool,
        String,
        Struct,
    }
    public class PeakObject
    {
        // standart values:
        public    int IntValue    { get; set; }
        public double DoubleValue { get; set; }
        public   bool BoolValue   { get; set; }
        public string StringValue { get; set; }
        public PeakObject[] StructValue { get; set; } // array / struct / ...


    }
}
