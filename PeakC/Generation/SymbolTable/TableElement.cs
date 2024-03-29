﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation
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
        public string Name { get { return NameToken?.Content; } }

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
        public VariableIdTracker IdTracker { get; private set; } 
        public VariableTableElement(SymbolTable st, Token name, SemanticType type)
        {
            this.IdTracker = new VariableIdTracker(st, convertTypeToSize(type));
            this.NameToken = name;
            this.Type = type;
        }

        public VariableTableElement(VariableIdTracker tracker, Token name, SemanticType type) 
        {
            this.NameToken = name;
            this.Type = type;
            this.IdTracker = tracker;
        }
        private static int convertTypeToSize(SemanticType type)
        {
            switch (type.Type)
            {
                case Generation.Type.Bool:
                    return 1;
                case Generation.Type.Int:
                case Generation.Type.Method:
                case Generation.Type.Double:
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
        public MethodSemanticType MethodSignature { get; set; }
        public string Label { get; set; }
        public MethodContextReferenceElement ExternContextRef { get; set; }
        //public Struct
    }

    class MethodContextReferenceElement : TableElement
    {
        public SymbolTable Context { get; set; }
        public MemoryIdTracker IdTracker { get; private set; }

        public MethodContextReferenceElement(SymbolTable st)
        {
            Source = st;
            IdTracker = new MemoryIdTracker(st, size: 8);
        }
    }
}
