using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.CodeGeneration
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
