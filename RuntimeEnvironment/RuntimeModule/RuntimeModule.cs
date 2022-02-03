using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    [Serializable]
    public class RuntimeModule
    {
        public Constant[] Constant { get; set; }
        public MethodDescription[] Methods { get; set; }
    }
}
