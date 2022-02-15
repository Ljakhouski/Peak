﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.CodeGeneration
{
    class TableElement
    {
        public string Name { get; set; }
        public int OffsetAddress { get; set; } = -1; // '-1' - has not address
        
        public SymbolType Type { get; set; }

        public SymbolTable Ref { get; set; }
        public Token Info { get; set; } // meta-data for error sending
        public Node InfoNode { get; set; }
        public MethodElement MethodInfo { get; set; }
        public SymbolTable MethodContextTable { get; set; }
        public int MethodNestLevel { get; set; } // for deployment context-references 
    }
    class MethodElement
    {
        public int MethodAddress { get; set; }
        public bool IsNative { get; set; }
        public string NativeMethodName { get; set; } // dynamic address
    }
    

}
