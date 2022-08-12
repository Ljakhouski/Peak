using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.InterpreterCodeGeneration
{
    static class IDGenerator
    {
        private static int refId = -1;

        public static int GetRefId()
        {
            refId++;
            return refId;
        }
    }
}
