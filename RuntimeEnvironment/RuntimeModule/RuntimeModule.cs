using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    [Serializable]
    public class RuntimeModule
    {
        public MethodDescription[] Methods { get; set; }
    }
}
