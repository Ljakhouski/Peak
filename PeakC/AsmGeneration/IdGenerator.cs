using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class IdGenerator
    {
        private static int memIdCounter = -1;
        private static int idCounter = 90000;
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
    }
}
