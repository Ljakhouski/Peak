using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.CodeGeneration
{
    class TableElement
    {
        public string Name { get; set; }
        public int OffsetAddress { get; set; } = -1; // has not address
        public SymbolType Type { get; set; }

        public SymbolTable Ref { get; set; }
        public Token Info { get; set; } // meta-data for error sending
        public Node InfoNode { get; set; }

        public SymbolTable MethodContextTable { get; set; }
    }
}
