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
        public int Id { get; private set; } = IdGenerator.GenerateId();
        public SymbolTable Source{ get; set; } // the place where he is 
        public SemanticType Type { get; set; }

        public Token NameToken { get; set; }
        public string Name { get { return NameToken.Content; } }

        public override bool Equals(object obj)
        {
            if (obj is null || obj is TableElement == false)
                return false;
            if ((obj as TableElement).Id == this.Id)
                return true;
            return false;
        }
        public static bool operator ==(TableElement e1, TableElement e2)
        {
            return e1.Equals(e2);
        }
        public static bool operator !=(TableElement e1, TableElement e2)
        {
            return !e1.Equals(e2);
        }
    }

    class VariableTableElement : TableElement
    {
        public MemoryIdTracker MemoryId { get; private set; } 
        public VariableTableElement(SymbolTable st, Token name, SemanticType type)
        {
            this.MemoryId = new MemoryIdTracker(st, convertTypeToSize(type));
            this.NameToken = name;
            this.Type = type;
        }

        private static int convertTypeToSize(SemanticType type)
        {
            switch (type.Type)
            {
                case AsmGeneration.Type.Bool:
                    return 1;
                case AsmGeneration.Type.Int:
                case AsmGeneration.Type.Method:
                case AsmGeneration.Type.Double:
                    return 8;
                default:
                    throw new CompileException();
            }
        }
    }
    class ConstTableElement : TableElement
    {
        public Token ConstValue { get; set; }
    }
    enum CallConvention
    {
        __stdcall,
        __ccall,
        x64_win,
        x86_64_linux 
    }
    class MethodTableElement : TableElement
    {
        public bool IsDllImportMethod { get; set; } = false;
        public CallConvention Convention { get; set; } = CallConvention.x64_win;
        public SemanticType MethodSignature { get; set; }

        public MethodContextReferenceElement ExternContextRef { get; set; }
        //public Struct
    }

    class MethodContextReferenceElement : TableElement
    {
        public SymbolTable Context { get; set; }
        public MemoryIdTracker MemoryId { get; private set; }

        public MethodContextReferenceElement(SymbolTable st)
        {
            Source = st;
            MemoryId = new MemoryIdTracker(st, size: 8);
        }
    }
}
