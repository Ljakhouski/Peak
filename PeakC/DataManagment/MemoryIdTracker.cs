using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
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
        /*** optional ***/
        //public MemoryPositonInfo Position { get; set; } // only for sync between code-generator and MemoryAllocator


        /***  about registers  ***/
        public bool ExistInRegisters
        {
            get
            {
                var regMap = this.allocator.RegisterMap;

                foreach (RegisterMapElement e in regMap)
                {
                    if (this == e.ContainedData)
                        return true;
                }
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

        public RegisterName Register
        {
            get
            {

                if (this.ExistInRegisters)
                {
                    var regMap = this.allocator.RegisterMap;

                    foreach (RegisterMapElement e in regMap)
                    {
                        if (this == e.ContainedData)
                            return e.Register;
                    }
                }

                throw new CompileException();
            }
        }


        /***  about stack  ***/
        public bool ExistInStack
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
        }

        public void Free()
        {
            FreeFromRegister();
            FreeFromStack();
        }

        public void FreeFromRegister()
        {
            this.allocator.SetRegisterFree(this.Register);
        }

        public void FreeFromStack()
        {
            foreach (var e in this.allocator.StackModel)
                if (this == e.ContainedData)
                    e.Free();
        }

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

        public MemoryIdTracker(SymbolTable st, int size)
        {
            this.Id = IdGenerator.GenerateMemoryId();
            this.allocator = st.MemoryAllocator;
            this.Size = size;
        }
        /*public MemoryIdTracker(SymbolTable st, int size, int alignment)
        {
            this.Id = IdGenerator.GenerateMemoryId();
            this.allocator = st.MemoryAllocator;
            this.Size = size;
           // this.Alignment = alignment;
        }*/

        private MemoryAllocator allocator;

        public static bool operator ==(MemoryIdTracker id1, MemoryIdTracker id2)
        {
            if (id1 is null == false)
                return id1.Equals(id2);
            else
                return id2.Equals(id1);
        }

        public static bool operator !=(MemoryIdTracker id1, MemoryIdTracker id2)
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
}
