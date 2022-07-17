using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    
    /*    
          
                                         Prefixs in methods: 
                        
                      Move-prefix --- is the moving CONSIST data from <?> to <?>

          Place-prefix --- is the placing new data to <?>, witch not consist in the stack or in registers 

               Set-prefix --- is the manual writing MemoryDataId to the register map ot stack
     */

    class MemoryDataId // object with info about memory-field in stack or in registers (or in heap-memory)
    {
        public int Id { get; private set; } // only for handle searching in register map or in stack-model
        public int Size { get; set; } // in bytes, size of data-type (without alligment)
        public int Alignment { get { return this.Size; } } 
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
                    if (e.ContainedData == this)
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

        public int StackOffset
        {
            get
            {
                var stack = this.allocator.StackModel;

                int offset = 0;

                for (int i = 0; i < stack.Count; i++)
                {
                    if (stack[i].IsFree() == false && this == stack[i].ContainedData)
                        return offset;
                    
                    offset += stack[i].Offset;
                }
                throw new CompileException();
            }
        } // offset regarding highest of frame address // NOT NOW: *offset regarding EBP address*
        public MemoryDataId(SymbolTable st)
        {
            this.Id = IdGenerator.GenerateId();
            this.allocator = st.MemoryAllocator;
        }

        private MemoryAllocator allocator;

        public static bool operator ==(MemoryDataId id1, MemoryDataId id2)
        {
            return id1.Equals(id2);
        }

        public static bool operator !=(MemoryDataId id1, MemoryDataId id2)
        {
            return !id1.Equals(id2);
        }

        public override bool Equals(object obj)
        {
            if (obj is MemoryDataId)
            {
                if (this.Id == (obj as MemoryDataId).Id)
                    return true;
            }
            return false;
        }
    }

    static class IdGenerator
    {
        static int counter = -1;
        public static int GenerateId()
        {
            counter++;
            return counter;
        }
    }

    class RegisterMapElement
    {
        public RegisterName Register { get; private set; }

        private MemoryDataId dataId = null;
        public MemoryDataId ContainedData { get { UsageNumber++; return dataId; } set { this.dataId = value; UsageNumber++; } }

        public int UsageNumber { get; private set; }
        //public int CounterOfUsing { get; set; } = 0; // for optimising

        public RegisterMapElement(RegisterName name)
        {
            this.Register = name;
        }

        public void Free()
        {
            this.dataId = null;
        }
    }

    class MemoryAreaElement
    {
        public MemoryDataId ContainedData { get; set; }
        public int Size { get; set; } // actual size in memory (data-size + alligment)
        public int Offset
        {
            get
            {
                int offset = 0;
                for (int i = 0; i < Allocator.StackModel.Count; i++)
                {
                    if (Allocator.StackModel[i].ContainedData == this.ContainedData)
                    {
                        return offset;
                    }
                    else
                        offset += Allocator.StackModel[i].Size;
                }

                throw new CompileException();
            }
        }

        public MemoryAllocator Allocator { get; private set; }
        public bool IsFree()
        {
            return ContainedData is null;
        }

        public void Free()
        {
            this.ContainedData = null;
        }
        public MemoryAreaElement(MemoryAllocator allocator)
        {
            this.Allocator = allocator;
        }
    }

    class MemoryAllocator
    {
        public SymbolTable NativeSymbolTable { get; set; }
        public int BasePointsAddress { get; set; } // in bytes, is the address to witch bp/ref point

        public List<MemoryAreaElement> StackModel = new List<MemoryAreaElement>();

        //public int RSP_frameOffset { get; set; }
        public List<RegisterMapElement> RegisterMap = new List<RegisterMapElement>() // element can be null if he is not exist in stack now
        {
            new RegisterMapElement(RegisterName.RAX),
            new RegisterMapElement(RegisterName.RBX),
            new RegisterMapElement(RegisterName.RCX),
            new RegisterMapElement(RegisterName.RDX),
            new RegisterMapElement(RegisterName.RSI),
            new RegisterMapElement(RegisterName.RDI),
                                  
            new RegisterMapElement(RegisterName.R8 ),
            new RegisterMapElement(RegisterName.R9 ),
            new RegisterMapElement(RegisterName.R10),
            new RegisterMapElement(RegisterName.R11),
            new RegisterMapElement(RegisterName.R12),
            new RegisterMapElement(RegisterName.R13),
            new RegisterMapElement(RegisterName.R14),
            new RegisterMapElement(RegisterName.R15),

        };

        public List<RegisterMapElement> SSERegisterMap = new List<RegisterMapElement>()
        {
            new RegisterMapElement(RegisterName.XMM0),
            new RegisterMapElement(RegisterName.XMM1),
            new RegisterMapElement(RegisterName.XMM2),
            new RegisterMapElement(RegisterName.XMM3),
            new RegisterMapElement(RegisterName.XMM4),
            new RegisterMapElement(RegisterName.XMM5),
            new RegisterMapElement(RegisterName.XMM6),
            new RegisterMapElement(RegisterName.XMM7)
        };

        /*public void PlaceInRegister(MemoryDataId memory, AsmMethod method)
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (e.DataId is null)
                {
                    //memory. = true, Register = e.Register };
                    e.DataId = memory;
                }
            }

            // if all registers is busy:

            foreach(MemoryDataId i in StackModel)
            {
                //if
                throw new NotImplementedException();
            }
        }*/

        public RegisterName GetFreeRegister()
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (e.ContainedData is null)
                {
                    return e.Register;
                }
            }

            var register = getOldestRegister();

            var freeElement = getFreeStackArea(register.ContainedData.Size, register.ContainedData.Alignment);

            // mov [rbp + offset], r?x

            this.NativeSymbolTable.MethodCode.Emit
                (InstructionName.Mov,
                new Operand()
                {
                    IsGettingAddress = true,
                    RegisterName = register.Register,
                    DataSizeExist = true,
                    Size = GetDataSizeName(register.ContainedData.Size),
                    Offset = CalculateLocalOffset(freeElement.Offset, NativeSymbolTable)
                },
                register.Register);

            register.Free(); // it is free now!

            return register.Register;
        }

        private RegisterMapElement getOldestRegister()
        {
            int minimalUsage = 0;

            foreach(RegisterMapElement e in RegisterMap)
                if (e.UsageNumber < minimalUsage)
                    minimalUsage = e.UsageNumber;

            foreach (RegisterMapElement e in RegisterMap)
                if (e.UsageNumber == minimalUsage)
                    return e;

            throw new CompileException();
        }
        private MemoryAreaElement getFreeStackArea(int areaSize, int alignment)
        {
            defragmentateStackModel();

            var freeSpaces = new List<int>();

            for (int i = 0; i < StackModel.Count; i++)
                if (StackModel[i].IsFree() && StackModel[i].Size >= areaSize)
                    freeSpaces.Add(i);




            if (freeSpaces.Count > 0)
            {

                int minimalSize = 0;
                MemoryAreaElement minimalArea = null;

                // get the element with minimal area size

                foreach (int i in freeSpaces)
                    if (StackModel[i].Size < minimalSize)
                    {
                        minimalSize = StackModel[i].Size;
                        minimalArea = StackModel[i];
                    }


                



                if (minimalArea.Size == areaSize)
                {
                    StackModel[freeSpaces[0]].ContainedData = new MemoryDataId(NativeSymbolTable);
                    return StackModel[freeSpaces[0]];
                }
                else
                {
                    return selectFreeMemoryAreaInStack(freeSpaces[0], areaSize);
                }
            }
            else
            {
                if (GetFrameSize() % alignment != 0) 
                {

                }
                StackModel.Add(new MemoryAreaElement(this) { Size = areaSize });
                return StackModel[StackModel.Count - 1];
            }
        }

        // just erase memory area in stack-map with size and with offset in args

        // if the mem-area in middle of free-area, then devide one area on three areas
        // if offset and size equal free-area offset and size then get this free area

        private MemoryAreaElement selectFreeMemoryAreaInStack(int offset, int size)
        {
            for (int i = 0; i<StackModel.Count; i++)
            {
                if (StackModel[i].Offset >= offset && StackModel[i].Offset <= offset+size)
                {
                    int firstSize = StackModel[i].Offset - offset;
                    int secondSize = size;
                    int thirdSize = (StackModel[i].Offset + StackModel[i].Size) - (offset+size);

                    int firstOffset = StackModel[i].Offset;
                    int secondOffset = offset;
                    int thirdOffset = secondOffset+secondSize;

                    
                    StackModel.RemoveAt(i);
                    int insertIndex = i;

                    if (firstSize != 0)
                    {
                        var firstArea = new MemoryAreaElement(this) { Size = firstSize};
                        StackModel.Insert(insertIndex, firstArea);
                        insertIndex++;
                    }

                    // paste secont element
                    var secondArea = new MemoryAreaElement(this) { Size = secondSize };
                    StackModel.Insert(insertIndex, secondArea);
                    insertIndex++;

                    if (thirdSize != 0)
                    {
                        var thirdArea = new MemoryAreaElement(this) { Size = thirdSize};
                        StackModel[insertIndex] = thirdArea;
                    }

                    return secondArea;
                }
            }

            throw new CompileException("stack-erasing error");
        }
       /* private MemoryAreaElement devideFreeMemAreaIntoTwo(int element, int size)
        {
            int oldSize = StackModel[element].Size;

            int newElementSize = oldSize - size;
            StackModel.Insert(element + 1, new MemoryAreaElement() { Size = newElementSize });

            return StackModel[element]; // return old element but with new size
        }*/
       public int GetFrameSize()
        {
            int size = 0;
            foreach (var e in StackModel)
                size += e.Size;
            return size;
        }

        private void defragmentateStackModel()
        {
            // can be free-spaces-cutting consist
            for (int i = 0; i < StackModel.Count; i++)
            {
                if (StackModel[i].IsFree())
                {
                    if (i < StackModel.Count - 1)
                    {
                        if (StackModel[i + 1].IsFree())
                        {
                            StackModel[i].Size += StackModel[i + 1].Size;
                            StackModel.RemoveAt(i + 1);
                            defragmentateStackModel();
                        }
                    }
                }
            }
        }

        

        public int CalculateLocalOffset(TableElement id)
        {
            var bpOffset = this.NativeSymbolTable.MemoryAllocator.BasePointsAddress;

            if (id is MethodContextReferenceElement)
            {
                if ((id as MethodContextReferenceElement).Id.ExistInRegisters)
                    throw new CompileException();
                else
                {
                    var offset = (id as MethodContextReferenceElement).Id.StackOffset;

                    return offset - bpOffset;
                }

            }
            else if (id is VariableTableElement)
            {
                if ((id as VariableTableElement).Id.ExistInRegisters)
                    throw new CompileException();
                else
                {
                    var offset = (id as VariableTableElement).Id.StackOffset;

                    return offset - bpOffset;
                }
            }
            else
                throw new CompileException();
        }
        public int CalculateLocalOffset(int frameOffset, SymbolTable st)
        {
            return frameOffset - st.MemoryAllocator.BasePointsAddress;
        }

        public void SetIdToFreeRegister(MemoryDataId id, RegisterName outputRegister)
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (e.Register == outputRegister)
                {
                    if (e.ContainedData is null)
                    {
                        e.ContainedData = id;
                        return;
                    }
                    else
                        throw new CompileException();
                }
            }
            throw new CompileException();
        }

        public void PlaceInStack(MemoryDataId idInRegister, int alligment = 8)
        {
            var area = getFreeStackArea(idInRegister.Size, alligment);

            this.NativeSymbolTable.MethodCode.Emit(
                InstructionName.Mov,
                new Operand()
                {
                    RegisterName = RegisterName.RBP,
                    DataSizeExist = true,
                    Size = GetDataSizeName(idInRegister.Size),
                    IsGettingAddress = true,
                    Offset = area.Offset
                }
                ,
                idInRegister.Register
            );
        }
        
        public void MoveToRegister(MemoryDataId data)   
        {
            if (data.ExistInRegisters)
                return;

            var freeReg = this.GetFreeRegister();

            this.NativeSymbolTable.MethodCode.Emit(
                InstructionName.Mov,
                freeReg,
                new Operand()
                {
                    DataSizeExist = true,
                    IsGettingAddress = true,
                    RegisterName = RegisterName.RBP,
                    Size = GetDataSizeName(data.Size),
                    Offset = data.StackOffset
                }
                );

            foreach (var e in StackModel)
                if (e.ContainedData == data)
                    e.Free();

            foreach (var e in RegisterMap)
                if (e.Register == freeReg)
                    e.ContainedData = data;
        }

        public void MoveToSSE_register(MemoryDataId data) { }

       /* public void MoveRegisterToStack(MemoryDataId data, MemoryDataId stackPlace, SymbolTable st)
        {
            if (data.ExistInSSERegisters)
            {
                // movs qword [rbp-8], rax
                st.MethodCode.Emit(InstructionName.Movss,
                    new Operand()
                    {
                        DataSizeExist = true,
                        IsGettingAddress = true,
                        Size = DataSize.QWord,
                        Offset = CalculateLocalOffset(stackPlace.StackOffset, st)
                    },

                    data.Register
                );

            }
            else
            {
                st.MethodCode.Emit(InstructionName.Mov,
                    new Operand()
                    {
                        DataSizeExist = true,
                        IsGettingAddress = true,
                        Size = GetDataSizeName(data.Size),
                        Offset = CalculateLocalOffset(stackPlace.StackOffset, st)
                    },
                    data.Register
                );

              
            }
        }*/

        /*public void MoveSSEDataToStack(MemoryDataId stackPlace, MemoryDataId data, SymbolTable st)
        {

        }*/

        public DataSize GetDataSizeName(int bytes)
        {
            switch (bytes)
            {
                case 1:
                    return DataSize.Byte;
                case 2:
                    return DataSize.Word;
                case 4:
                    return DataSize.Dqword;
                case 8:
                    return DataSize.QWord;
                default:
                    throw new CompileException();
            }
        }
    }
}
