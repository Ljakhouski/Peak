using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    class GenResult
    {
        //public Operand Operand { get; set; }

        public SemanticType ResultType { get; set; }
        public MemoryDataId ReturnDataId { get; set; }

    }

    class EmptyGenResult : GenResult
    {

    }
    class ConstantResult : GenResult
    {
        /*
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }*/
        public Token ConstValue { get; set; }
    }
}
