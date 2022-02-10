using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    [Serializable]
    public class MethodDescription
    {
        public string Name { get; set; } /* meta-data, for disassembling */


        public int LocalVarsArraySize { get; set; }
        public Instruction[] Code { get; set; }

    }
}
