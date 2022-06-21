using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    class MemoryPositonInfo // for description position in stack or in register-map of every MemoryInstance-object
    {
        public bool IsRegisterPlace { get; set; }
        public RegisterName Register { get; set; }
        public int StackOffset { get; set; } // offset regarding EBP address
    }

    class MemoryInstance // object of memory-field in stack or in registers (or in heap-memory)
    {
        public int Id { get; set; }
        public int Size { get; set; } // byte of numbers (for one x64 word - 8 bytes)
        public MemoryPositonInfo Position { get; set; } // only for sync between code-generator and MemoryAllocator
    }

    class RegisterMapElement
    {
        public RegisterName   Register;
        public MemoryInstance MemoryInstance;
    };

    class MemoryAllocator
    {
        public List<MemoryInstance> StackModel = new List<MemoryInstance>();
        public List<RegisterMapElement> RegisterMap = new List<RegisterMapElement>()
        {
            new RegisterMapElement { Register = RegisterName.RAX, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.RBX, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.RCX, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.RDX, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.RSI, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.RDI, MemoryInstance = null },

            new RegisterMapElement { Register = RegisterName.R8,  MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R9,  MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R10, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R11, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R12, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R13, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R14, MemoryInstance = null },
            new RegisterMapElement { Register = RegisterName.R15, MemoryInstance = null },
            
        };

        public void PlaceInRegister(MemoryInstance memory, AsmMethod method)
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (e.MemoryInstance is null)
                {
                    memory.Position = new MemoryPositonInfo() { IsRegisterPlace = true, Register = e.Register };
                    e.MemoryInstance = memory;
                }
            }

            // if all registers is busy:

            foreach(MemoryInstance i in StackModel)
            {
                //if
            }
        }
    }
}
