using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    class SymbolTable
    {
        public int ID { get; }

        public SymbolTable()
        {
            
        }
    }


    class MethodSymbolTable : SymbolTable
    {
        public string Name { get; set; }
        public Token TokenName { get; set; }
        //public MethodNode MethodNode { get; set; }

    }
}
