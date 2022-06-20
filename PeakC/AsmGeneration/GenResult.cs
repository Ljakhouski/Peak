using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    class GenResult
    {
        public Operand Operand { get; set; }
        public List<AsmInstruction> Code { get; set; } = new List<AsmInstruction>();

        public ReturnType ExprType { get; set; }

    }
}
