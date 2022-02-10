using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    public struct Instruction
    {
        public InstructionName Name;
        public int[] Operands;
    }
}
