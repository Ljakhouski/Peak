﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Peak.AsmGeneration
{
    class SymbolTable
    {
        public SymbolTable Next { get; set; }
        public SymbolTable Prev { get; set; }
        public virtual GlobalSymbolTable GlobalTable { get { return Prev.GlobalTable; } }
        public virtual AsmModel MainAssembly { get { return Prev.MainAssembly; } }

        public List<TableElement> Data = new List<TableElement>();

        public bool ContainsInAllSpaces(Token name)
        {
            foreach (TableElement e in Data)
            {
                if (e.Name == name.Content)
                {
                    return true;
                }
            }
            if (Prev != null /*&& this is StructureSymbolTable == false*/)
                return Prev.ContainsInAllSpaces(name);
            else
                return false;
        }

        public TableElement GetSymbolFromAllSpaces(Token name)
        {
            foreach (TableElement e in this.Data)
            {
                if (e.Name == name.Content)
                    return e;
            }

            if (Prev != null /*&& this is StructureSymbolTable*/)
            {
                return Prev.GetSymbolFromAllSpaces(name);
            }
            else
                return null;
        }

        public void RegisterMethod(MethodTableElement tableElement)
        {
            tableElement.Source = this;
            this.Data.Add(tableElement);
        }

        /* most important method for allocate new data in stack and in symbol table */
        public void RegisterVariable(VariableTableElement e)
        {
            if (e.Type.Type == Type.Int)
                e.Id.Size = 4;
            else if (e.Type.Type == Type.Double)
                e.Id.Size = 8;
            else if (e.Type.Type == Type.Bool)
                e.Id.Size = 1;
            else
                throw new CompileException();

            //e.Id.Alignment = e.Id.Size;
            this.MemoryAllocator.AllocateInStack(e.Id, e.Id.Alignment);
            this.Data.Add(e);
        }

        public void Emit(string instruction)
        {
            this.MethodCode.Emit(instruction);
        }

        public void Emit(string instruction, string comment)
        {
            this.MethodCode.Emit(instruction, comment);
        }
        public TableElement GetFromMethodContext(Token name)
        {
            foreach (TableElement e in Data)
            {
                if (e.Name == name.Content && e is VariableTableElement)
                {
                    return e;
                }
            }
            if (Prev != null && this is StructureSymbolTable == false && this is MethodSymbolTable == false)
                return Prev.GetFromMethodContext(name);
            else
                return null;
        }

        public MethodContextReferenceElement GetMethodContextRef()
        {
            foreach (TableElement e in Data)
            {
                if (e is MethodContextReferenceElement)
                {
                    return e as MethodContextReferenceElement;
                }
            }
            if (Prev != null && this is StructureSymbolTable == false && this is MethodSymbolTable /* can be removed*/)
                return Prev.GetMethodContextRef();
            else
                return null;
        }
        //public int ID { get; }

        public SymbolTable()
        {

        }

        public virtual MemoryAllocator MemoryAllocator 
        {   
            get         // TODO: make exception for struct-symbol table
            {
                do
                {
                    var st = this.Prev;

                    if (st is MethodSymbolTable)
                        return /*(st as MethodSymbolTable)*/st.MemoryAllocator;
                }while (Prev != null);

                throw new CompileException();
            }
            set
            {
                do
                {
                    var st = this.Prev;

                    if (st is MethodSymbolTable)
                        st.MemoryAllocator = value;
                } while (Prev != null);
            }
        }

        public virtual AsmMethod MethodCode
        {
            get
            {
                do
                {
                    var st = this.Prev;

                    if (st is MethodSymbolTable)
                        return /*(st as MethodSymbolTable)*/st.MethodCode;
                } while (Prev != null);

                throw new CompileException();
            }
            set
            {
                do
                {
                    var st = this.Prev;

                    if (st is MethodSymbolTable)
                        st.MethodCode = value;
                } while (Prev != null);
            }
        }

    }

    class GlobalSymbolTable : MethodSymbolTable
    {
        private AsmModel mainAssembly_ = new AsmModel();
        public override AsmModel MainAssembly { get { return mainAssembly_; } } 
        public override GlobalSymbolTable GlobalTable { get { return this; } } 
        private List<string> loadetFiles = new List<string>();
        public bool IsNewFile(string fileName)
        {
            if (loadetFiles.Contains(Path.GetFullPath(fileName)))
                return false;
            return true;
        }

        public void RegisterFile(string file)
        {
            loadetFiles.Add(file);
        }

        public void RegisterSymbol(TableElement tableElement)
        {
            tableElement.Source = this;
            //tableElement.OffsetAddress = calculateNewOffsetAddress();
            Data.Add(tableElement);
            //if ()
        }
    }

    class StructureSymbolTable : SymbolTable
    {

    }


    class MethodSymbolTable : SymbolTable
    {
        //public string Name { get; set; }
        //public Token TokenName { get; set; }
        //public MethodNode MethodNode { get; set; }


        public override MemoryAllocator MemoryAllocator { get; set; }
        public override AsmMethod MethodCode { get; set; }

        public MethodSymbolTable()
        {
            this.MemoryAllocator = new MemoryAllocator(this);
            this.MethodCode = new AsmMethod();

        }
        public void Emit(string instruction)
        {
            this.MethodCode.Emit(instruction);
        }
    }
}
