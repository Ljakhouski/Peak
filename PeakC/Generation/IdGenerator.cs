using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation
{
    static class IdGenerator
    {
        private static int memIdCounter = -1;
        private static int idCounter = 900000;
        private static int stIdCounter = 700000;
        private static int methodCounter = 500000;
        private static int labelCounter = 40000;
        public static int GenerateMemoryId()
        {
            memIdCounter++;
            return memIdCounter;
        }

        public static int GenerateId()
        {
            idCounter++;
            return idCounter;
        }

        public static int GenerateSymbolTableId()
        {
            stIdCounter++;
            return stIdCounter;
        }

        public static string GenerateLabel()
        {
            labelCounter++;
            return $"L_GEN{labelCounter}";
        }

        public static int GenerateMethodId()
        {
            methodCounter++;
            return methodCounter;
        }
    }
}
