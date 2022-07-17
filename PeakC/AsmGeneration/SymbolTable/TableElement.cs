using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    /*enum TableElementType
    {
        Variable,
        Constant,
        Method,
        Struct,

    }*/
    class TableElement
    {

        public SymbolTable Source{ get; set; } // the place where he is 
        public SymbolType Type { get; set; }

        public Token NameToken { get; set; }
        public string Name { get { return NameToken.Content; } }
    }

    class VariableTableElement : TableElement
    {
        public MemoryDataId Id { get; private set; } 
        public VariableTableElement(SymbolTable st, Token name, SymbolType type)
        {
            this.Id = new MemoryDataId(st);
            this.NameToken = name;
            this.Type = type;
        }

    }
    class ConstTableElement : TableElement
    {
        public Token ConstValue { get; set; }
    }
    enum CallConvention
    {
        __stdcall,
        __ccall
    }
    class MethodTableElement : TableElement
    {
        public bool IsDllImportMethod { get; set; } = false;
        public CallConvention Convention { get; set; } = CallConvention.__stdcall;
        public SymbolType MethodSignature { get; set; }

    }

    class MethodContextReferenceElement : TableElement
    {
        public SymbolTable Context { get; set; }
        public MemoryDataId Id { get; private set; }

        public MethodContextReferenceElement(SymbolTable st)
        {
            Source = st;
            Id = new MemoryDataId(st);
        }
    }
}
