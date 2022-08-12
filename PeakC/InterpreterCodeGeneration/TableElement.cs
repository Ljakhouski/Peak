using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.InterpreterCodeGeneration
{
    class TableElement
    {
        public string Name { get; set; }
        public int OffsetAddress { get; set; } = -1; // '-1' - has not address
        
        public SemanticType Type { get; set; }

        public SymbolTable Ref { get; set; } // authomatically
        public int ReferingContextId { get; internal set; } // id for equalling method and struct references (id from method memory context or id from struct memory context)

        public Token Info { get; set; } // meta-data for error sending
        public Node InfoNode { get; set; }
        //public MethodElement MethodInfo { get; set; }
        public SymbolTable MethodContextTable { get; set; }

        
    }
    class MethodTableElement : TableElement
    {
        public int MethodAddress { get; set; }
        public bool IsNative { get; set; }
        public string NativeMethodName { get; set; } // dynamic address
    }
    

}
