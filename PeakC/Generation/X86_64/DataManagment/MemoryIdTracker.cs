using Peak.PeakC;
using Peak.PeakC.Generation.X86_64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peak.PeakC.Generation
{

    /* the object that describes the location of data in registers or on the stack.
     * Works ONLY with top stack-frame. 
     * Used for code-generation to track data */

    class MemoryIdTracker
    {
        public int Id { get; private set; } // only for handle searching in register map or in stack-model
        public int Size { get; set; } // in bytes, size of data-type (without alligment)
        public int Alignment { get { return this.Size; } }
        public bool IsSSE_Element { get; set; } = false; // for double/float type

        protected MemoryAllocator allocator;

        /***  about registers  ***/
       /* public bool ExistInRegisters
        {
            get
            {
                var regMap = this.allocator.RegisterMap;

                foreach (RegisterMapElement e in regMap)
                {
                    if (this == e.ContainedData)
                        return true;
                }

                if (IsRbp)
                    return true;

                return false;
            }
        }

        public bool ExistInSSERegisters
        {
            get
            {
                var regMap = this.allocator.SSERegisterMap;

                foreach (RegisterMapElement e in regMap)
                {
                    if (e.ContainedData == this)
                        return true;
                }
                return false;
            }
        }
       */
        
        public static MemoryIdTracker FuncResult(SymbolTable st, bool isSSE = false)
        {
            if (isSSE)
            {
                foreach (var e in st.MemoryAllocator.SSERegisterMap)
                {
                    if (e.Register == RegisterName.xmm0)
                    {
                        e.ContainedData = new MemoryIdTracker(st, 8)
                        {
                            IsSSE_Element = true
                        };

                        return e.ContainedData;
                    }
                }
            }


            var e_ = (from el in st.MemoryAllocator.RegisterMap
                      where el != null && el.Register == RegisterName.rax
                      select el).ToArray()[0];

            foreach (var e in st.MemoryAllocator.RegisterMap)
            {
                if (e.Register == RegisterName.rax)
                {
                    e.ContainedData = new MemoryIdTracker(st, size: 8);
                    return e.ContainedData;
                }
            }

            throw new CompileException();
        }
        /*
        public RegisterName Register
        {
            get
            {

                if (this.allocator.ExistInRegisters(this) /*this.ExistInRegisters)
                {
                    if (IsRbp)
                        return RegisterName.rbp;

                    var regMap = this.allocator.RegisterMap;

                    foreach (RegisterMapElement e in regMap)
                    {
                        if (this == e.ContainedData)
                            return e.Register;
                    }
                }

                throw new CompileException();
            }
        }*/


        /***  about stack  ***/
        /*public bool ExistInStack
        {
            get
            {
                var stack = this.allocator.StackModel;

                foreach (MemoryAreaElement area in stack)
                {
                    if (this == area.ContainedData)
                        return true;
                }
                return false;
            }
        }*/
        /*
        public void Free()
        {
            FreeFromRegister();
            FreeFromStack();
        }

        public void FreeFromRegister()
        {
            this.allocator.SetRegisterFree(this.Register);
        }

        public virtual void FreeFromStack()
        {
            foreach (var e in this.allocator.StackModel)
                if (this == e.ContainedData)
                    e.Free();
        }*/
        /*
        public int Rbp_Offset
        {
            get
            {
                foreach (var e in this.allocator.StackModel)
                    if (this == e.ContainedData)
                        return e.Rbp_Offset;

                throw new CompileException();

                //return this.StackOffset - this.allocator.RBP_dataId.StackOffset;
            }
        }

        public int StackOffset
        {
            get
            {
                foreach (var e in this.allocator.StackModel)
                    if (e.ContainedData == this)
                        return e.StackOffset;

                throw new CompileException();
            }
        }
        */
        public bool IsRbp { get; set; } 

        public MemoryIdTracker(SymbolTable st, int size)
        {
            this.Id = IdGenerator.GenerateMemoryId();
            this.allocator = st.MemoryAllocator;
            this.Size = size;
        }
       

        public static bool operator == (MemoryIdTracker id1, MemoryIdTracker id2)
        {
            if (id1 is null == false)
                return id1.Equals(id2);
            else
                return id2.Equals(id1);
        }

        public static bool operator != (MemoryIdTracker id1, MemoryIdTracker id2)
        {
            return !id1.Equals(id2);
        }

        public override bool Equals(object obj)
        {
            if (obj is MemoryIdTracker)
            {
                if (this.Id == (obj as MemoryIdTracker).Id)
                    return true;
            }
            return false;
        }
    }

    // tracker only for local/global variables in stack frames. To copy move data from it make new TrackId
    class VariableIdTracker : MemoryIdTracker
    {
        public VariableIdTracker(SymbolTable st, int size) : base(st, size)
        {
            // i'm hope it is normal
        }
    }
}
