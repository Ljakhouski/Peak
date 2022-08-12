using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    internal interface ICodeGeneration
    {
        public GenResult Generate(Node node);
    }
}
