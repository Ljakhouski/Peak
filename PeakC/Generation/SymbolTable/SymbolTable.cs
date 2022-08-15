using Peak.PeakC;
using Peak.PeakC.Generation.X86_64;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Peak.PeakC.Generation
{
    class SymbolTable
    {
        public SymbolTable Next { get; set; }
        public SymbolTable Prev { get; set; }
        public virtual GlobalSymbolTable GlobalTable { get { return Prev.GlobalTable; } }
        public virtual MethodSymbolTable MethodTable { get { return Prev.MethodTable; } }
        public bool ExistInMethodContext(TableElement element)
        {
            foreach(var e in this.Data)
            {
                if (element == e)
                    return true;
            }

            if (Prev != null && this is StructureSymbolTable == false && this is MethodSymbolTable == false)
                return Prev.ExistInMethodContext(element);
            else
                return false;
        }

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

        public TableElement GetSymbolFromVisibleSpaces(Token name)
        {
            foreach (TableElement e in this.Data)
            {
                if (e.Name == name.Content)
                    return e;
            }

            if (Prev != null /*&& this is StructureSymbolTable*/)
            {
                return Prev.GetSymbolFromVisibleSpaces(name);
            }
            else
                return null;
        }

        // return static method definition, which can be called
        public MethodTableElement GetVisibleMethodTableElement(Token id, SemanticType signature)
        {
            foreach (var e in this.Data)
                if (e is MethodTableElement && id.Content == e.Name && (e as MethodTableElement).MethodSignature == signature)
                    return e as MethodTableElement;
            if (this.Prev is null == false && this is StructureSymbolTable == false)
                return this.Prev.GetVisibleMethodTableElement(id, signature);
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
                e.IdTracker.Size = 4;
            else if (e.Type.Type == Type.Double)
                e.IdTracker.Size = 8;
            else if (e.Type.Type == Type.Bool)
                e.IdTracker.Size = 1;
            else
                throw new CompileException();

            //var id = new VariableIdTracker(this, e.MemoryId.Size);
            var memId = e.IdTracker;
            var stackArea = this.MemoryAllocator.AllocateAreaInStack(memId, memId.Alignment);
            stackArea.ContainedData = memId;
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
        public int Id { get; private set; } = IdGenerator.GenerateSymbolTableId();

        public override MemoryAllocator MemoryAllocator { get; set; }
        public override AsmMethod MethodCode { get; set; }
        public override MethodSymbolTable MethodTable { get { return this; } }
        public MethodSymbolTable()
        {
            this.MemoryAllocator = new MemoryAllocator(this);
            this.MethodCode = new AsmMethod();

        }
        public void Emit(string instruction)
        {
            this.MethodCode.Emit(instruction);
        }

        public void RegisterContextRef(MethodContextReferenceElement mRefElement)
        {
            var id = new MemoryIdTracker(this, size: 8);
            var e = this.MemoryAllocator.AllocateAreaInStack(id);
            e.ContainedData = id;
            this.Data.Add(mRefElement);
        }
    }
}
