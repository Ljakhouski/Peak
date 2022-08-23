using Peak.PeakC;
using Peak.PeakC.Generation.X86_64;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation
{

    /*    
          
                                         Prefixs in methods: 
                        
                      Move-prefix --- is the moving CONSIST data from <?> to <?>

          Allocate-prefix --- is the placing new data to <?>, witch not consist in the stack or in registers 

               Set-prefix --- is the manual writing MemoryIdTracker to the register map ot stack
     */

    class RegisterMapElement
    {
        public RegisterName Register { get; private set; }
        public bool IsBlocked { get; set; } = false;

        private MemoryIdTracker dataId = null;          // для конвертации типов использовать механизм конвертации типов. ДЛЯ ДАННЫХ: обрезка (cast) регистров происходит при размещении в памяти регистров с заданным размером. 
        public MemoryIdTracker ContainedData// при вычислении в зависимости типа записываем данные из памяти в регистр / разибраем с регистра данные. ПРИ НЕСТРОГОЙ ТИПИЗАЦИИ конвертируем.
                                            // в нашем случае данные просто запишутся из памяти согласно их размеру автоматом. для вычисления int32 используем eax, ebx, edx... для bool - al, bl,... и просто считаем все ими
                                            //
        {
            get
            {
                UsageNumber++; return dataId;
            }
            set
            {
                this.dataId = value;
                UsageNumber++;
            }
        }


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
        public MemoryIdTracker ContainedData { get; set; }
        public int Size { get; set; } // actual size in memory (data-size + alligment)

        public int Rbp_Offset
        {
            get
            {
                return this.StackOffset -  this.Allocator.GetStackOffset(this.Allocator.RBP_dataId);
            }
        }

        public int StackOffset
        {
            get
            {
                int absoluteOffset = 0;

                foreach (var e in this.Allocator.StackModel)
                {
                    absoluteOffset -= e.Size;

                    if (this == e)
                    {
                        return absoluteOffset;
                    }
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

    partial class MemoryAllocator
    {
        public SymbolTable NativeSymbolTable { get; set; }
        public MemoryIdTracker RBP_dataId { get; set; }

        public List<MemoryAreaElement> StackModel = new List<MemoryAreaElement>();

        public List<RegisterMapElement> RegisterMap = new List<RegisterMapElement>() // element can be null if he is not exist in stack now
        {
            new RegisterMapElement(RegisterName.rax),
            new RegisterMapElement(RegisterName.rbx),
            new RegisterMapElement(RegisterName.rcx),
            new RegisterMapElement(RegisterName.rdx),
            new RegisterMapElement(RegisterName.rsi),
            new RegisterMapElement(RegisterName.rdi),

            new RegisterMapElement(RegisterName.r8 ),
            new RegisterMapElement(RegisterName.r9 ),
            new RegisterMapElement(RegisterName.r10),
            new RegisterMapElement(RegisterName.r11),
            new RegisterMapElement(RegisterName.r12),
            new RegisterMapElement(RegisterName.r13),
            new RegisterMapElement(RegisterName.r14),
            new RegisterMapElement(RegisterName.r15),

        };

        public List<RegisterMapElement> SSERegisterMap = new List<RegisterMapElement>()
        {
            new RegisterMapElement(RegisterName.xmm0),
            new RegisterMapElement(RegisterName.xmm1),
            new RegisterMapElement(RegisterName.xmm2),
            new RegisterMapElement(RegisterName.xmm3),
            new RegisterMapElement(RegisterName.xmm4),
            new RegisterMapElement(RegisterName.xmm5),
            new RegisterMapElement(RegisterName.xmm6),
            new RegisterMapElement(RegisterName.xmm7)
        };

        public MemoryAllocator(SymbolTable table)
        {
            this.NativeSymbolTable = table;
        }

        // relocate data from register to make free
        public void FreeRegister(RegisterName register)
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (X86_64_Model.Equals(e.Register, register))
                {
                    if (e.ContainedData is null)
                        return;
                    else
                    {
                        var reg = this.GetFreeRegister();

                        if (reg == register)
                        {
                            SetRegisterFree(register);
                            return;
                        }
                        else
                        {
                            EmitMovRegisterToRegister(register, reg, 8, this.NativeSymbolTable);
                            //this.NativeSymbolTable.Emit($"mov {reg}, {register}");
                            SetRegister(e.ContainedData, reg);
                            //this.GetRegisterMapElement(reg).ContainedData = this.GetRegisterMapElement(register).ContainedData;
                            e.Free();
                            return;
                        }

                    }
                }
            }

            foreach (RegisterMapElement e in SSERegisterMap)
            {
                if (e.Register == register)
                {
                    if (e.ContainedData is null)
                        return;
                    else
                    {
                        var reg = this.GetFreeSSERegister();

                        if (reg == register)
                        {
                            this.GetRegisterMapElement(register).Free();
                            return;
                        }
                        else
                        {
                            EmitMovRegisterToRegister(register, reg, 8, this.NativeSymbolTable);
                            //this.NativeSymbolTable.Emit($"mov {reg}, {register}");
                            this.GetRegisterMapElement(reg).ContainedData = this.GetRegisterMapElement(register).ContainedData;
                            e.Free();
                            return;
                        }

                    }
                }
            }
        }

        public void SetRegisterFree(RegisterName r)
        {
            foreach (var e in RegisterMap)
                if (e.Register == r)
                {
                    e.Free();
                    return;
                }

            foreach (var e in SSERegisterMap)
                if (e.Register == r)
                {
                    e.Free();
                    return;
                }
        }

        public RegisterName GetFreeRegister()
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (e.ContainedData is null && e.IsBlocked == false)
                {
                    return e.Register;
                }
            }

            var register = getOldestRegister();
            var registerId = GetRegisterMapElement(register).ContainedData;
            var registerSize = registerId.Size;
            var registerAlignment = registerId.Alignment;
            var freeStackElement = getFreeStackArea(registerSize, registerAlignment);

            EmitMovRegisterToMemory(register, RegisterName.rbp, freeStackElement.Rbp_Offset, 8, this.NativeSymbolTable);

            SetRegisterFree(register); // it is free now!
            freeStackElement.ContainedData = registerId;
            return register;
        }
        public RegisterName GetFreeSSERegister()
        {
            foreach (RegisterMapElement e in SSERegisterMap)
            {
                if (e.ContainedData is null && e.IsBlocked == false)
                {
                    return e.Register;
                }
            }

            var register = getOldestSSERegister();
            var registerId = GetRegisterMapElement(register).ContainedData;
            var registerSize = registerId.Size;
            var registerAlignment = registerId.Alignment;
            var freeStackElement = getFreeStackArea(registerSize, registerAlignment);

            EmitMovRegisterToMemory(register, RegisterName.rbp, freeStackElement.Rbp_Offset, 8, this.NativeSymbolTable);
            
            SetRegisterFree(register); // it is free now!
            freeStackElement.ContainedData = registerId;
            return register;
        }

        public MemoryIdTracker GetNewIdInRegister(int size)
        {
            var reg = GetFreeRegister();
            var id = new MemoryIdTracker(this.NativeSymbolTable, size);
            SetIdToFreeRegister(id, reg);
            return id;
        }

        public void SetRegisterFree(GenResult res)
        {
            if (res is ConstantResult)
                return;

            SetRegisterFree(res.ReturnDataId);
        }

        public void SetRegisterFree(MemoryIdTracker t)
        {
            if (ExistInRegisters(t)|| ExistInSSERegisters(t))
                SetRegisterFree(GetRegister(t));
        }

        private RegisterName getOldestRegister()
        {
            int minimalUsage = Int32.MaxValue;

            foreach (RegisterMapElement e in RegisterMap)
                if (e.UsageNumber < minimalUsage && e.IsBlocked == false)
                    minimalUsage = e.UsageNumber;

            foreach (RegisterMapElement e in RegisterMap)
                if (e.UsageNumber == minimalUsage && e.IsBlocked == false)
                    return e.Register;

            throw new CompileException();
        }
        private RegisterName getOldestSSERegister()
        {
            int minimalUsage = Int32.MaxValue;

            foreach (RegisterMapElement e in SSERegisterMap)
                if (e.UsageNumber < minimalUsage && e.IsBlocked == false)
                    minimalUsage = e.UsageNumber;

            foreach (RegisterMapElement e in SSERegisterMap)
                if (e.UsageNumber == minimalUsage && e.IsBlocked == false)
                    return e.Register;

            throw new CompileException();
        }
        private MemoryAreaElement getFreeStackArea(int areaSize, int alignment)
        {
            defragmentateStackModel();

            var freeSpaces = new List<MemoryAreaElement>();

            for (int i = 0; i < StackModel.Count; i++)
                if (StackModel[i].IsFree() && StackModel[i].Size >= areaSize)
                    freeSpaces.Add(StackModel[i]);

            sortFreeSpaces();

            if (freeSpaces.Count > 0)
                foreach (var element in freeSpaces)
                {
                    if (AlignUpAbsolute(element.StackOffset, alignment) == element.StackOffset && element.Size == areaSize)
                        return element;
                    else
                        return expandStackModel();
                    //else if ()
                }

            return expandStackModel();


            void sortFreeSpaces()
            {
                MemoryAreaElement temp;

                for (int write = 0; write < freeSpaces.Count; write++)
                {
                    for (int sort = 0; sort < freeSpaces.Count - 1; sort++)
                    {
                        if (freeSpaces[sort].Size > freeSpaces[sort + 1].Size)
                        {
                            temp = freeSpaces[sort + 1];
                            freeSpaces[sort + 1] = freeSpaces[sort];
                            freeSpaces[sort] = temp;
                        }
                    }
                }
            }

            MemoryAreaElement expandStackModel()
            {
                if (GetFrameSize() % alignment != 0)
                {
                    var size = AlignUpAbsolute(GetFrameSize(), alignment) - GetFrameSize();
                    StackModel.Add(new MemoryAreaElement(this) { Size = size, ContainedData = null });
                    var ae = new MemoryAreaElement(this) { Size = areaSize };
                    StackModel.Add(ae);
                    return ae;
                }

                var e = new MemoryAreaElement(this) { Size = areaSize };
                StackModel.Add(e);
                return e;
            }
        }

        // just erase memory area in stack-map with size and with offset in args

        // if the mem-area in middle of free-area, then devide one area on three areas
        // if offset and size equal free-area offset and size then get this free area

        private MemoryAreaElement selectFreeMemoryAreaInStack(int offset, int size)
        {
            for (int i = 0; i < StackModel.Count; i++)
            {
                if (StackModel[i].StackOffset >= offset && StackModel[i].StackOffset <= offset + size)
                {
                    int firstSize = StackModel[i].StackOffset - offset;
                    int secondSize = size;
                    int thirdSize = (StackModel[i].StackOffset + StackModel[i].Size) - (offset + size);

                    int firstOffset = StackModel[i].StackOffset;
                    int secondOffset = offset;
                    int thirdOffset = secondOffset + secondSize;


                    StackModel.RemoveAt(i);
                    int insertIndex = i;

                    if (firstSize != 0)
                    {
                        var firstArea = new MemoryAreaElement(this) { Size = firstSize };
                        StackModel.Insert(insertIndex, firstArea);
                        insertIndex++;
                    }

                    // paste secont element
                    var secondArea = new MemoryAreaElement(this) { Size = secondSize };
                    StackModel.Insert(insertIndex, secondArea);
                    insertIndex++;

                    if (thirdSize != 0)
                    {
                        var thirdArea = new MemoryAreaElement(this) { Size = thirdSize };
                        StackModel[insertIndex] = thirdArea;
                    }

                    return secondArea;
                }
            }

            throw new CompileException("stack-erasing error");
        }

        public int GetFrameSize()
        {
            int size = 0;
            foreach (var e in StackModel)
                size += e.Size;
            return size;
        }

        public MemoryIdTracker GetMemoryIdTracker(RegisterName register)
        {
            return GetRegisterMapElement(register).ContainedData;
        }
        public RegisterMapElement GetRegisterMapElement(RegisterName register)
        {
            foreach (var e in this.RegisterMap)
                if (X86_64_Model.Equals(e.Register, register))
                    return e;
            foreach (var e in this.SSERegisterMap)
                if (e.Register == register)
                    return e;
            throw new CompileException();
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

        public void SetIdToFreeRegister(MemoryIdTracker id, RegisterName outputRegister)
        {
            foreach (RegisterMapElement e in RegisterMap)
            {
                if (X86_64_Model.Equals(e.Register, outputRegister))
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

            foreach (RegisterMapElement e in SSERegisterMap)
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
        public void SetRegister(MemoryIdTracker id, RegisterName register)
        {
            SetRegisterFree(register);
            SetIdToFreeRegister(id, register);
        }

        public MemoryAreaElement AllocateAreaInStack(MemoryIdTracker id, int alignment = 8)
        {
            return getFreeStackArea(id.Size, alignment);
        }

        // move to any r-registers or SSE registers depend of the data type
        public MemoryIdTracker MoveToAnyRegister(GenResult result)
        {
            if (result is ConstantResult)
            {
                return (result as ConstantResult).MoveToRegister(this.NativeSymbolTable).ReturnDataId;
            }
            else
                MoveToAnyRegister(result.ReturnDataId);
            return result.ReturnDataId;
        }

        // move to any r-registers or SSE registers depend of the data type
        public void MoveToAnyRegister(MemoryIdTracker data)
        {
            if (ExistInRegisters(data) || ExistInSSERegisters(data))
                return;
            else
            {
                RegisterName anyFreeRegister;

                if (data.IsSSE_Element)
                    anyFreeRegister = GetFreeSSERegister();
                else
                    anyFreeRegister = GetFreeRegister();

                generateMovToRegisterFromAllContexts(data, this.NativeSymbolTable, anyFreeRegister);
                SetRegister(data, anyFreeRegister);
            }
        }



        public static int AlignUpAbsolute(int i, int alignment)
        {
            int input = Math.Abs(i);

            while (input % alignment != 0)
                input++;

            return i < 0 ? (-1) * input : input;
        }

        public void MoveToRegister(MemoryIdTracker data, RegisterName register)
        {
            FreeRegister(register);

            if (ExistInRegisters(data) || ExistInSSERegisters(data))
            {
                EmitMovRegisterToRegister(GetRegister(data), register, data.Size, this.NativeSymbolTable);
            }
            else
            {
                generateMovToRegisterFromAllContexts(data, this.NativeSymbolTable, register);
            }

            SetRegister(data, register);
        }
        public void MoveToRegister(GenResult res, RegisterName register)
        {
            if (res is ConstantResult)
            {
                this.FreeRegister(register);  // TODO: rewrite

                if ((res as ConstantResult).ResultType.Type == Type.Int)
                    this.NativeSymbolTable.Emit($"mov {register}, {(res as ConstantResult).IntValue}");
                else if ((res as ConstantResult).ResultType.Type == Type.Bool)
                    this.NativeSymbolTable.Emit($"mov {register}, {(res as ConstantResult).BoolValue}");
                else if ((res as ConstantResult).ResultType.Type == Type.Bool)
                {
                    throw new CompileException("double not implemented");
                }

                SetIdToFreeRegister(res.ReturnDataId, register);
            }
            else
            {
                MoveToRegister(res.ReturnDataId, register);
            }
        }

        public void MoveToStack(MemoryIdTracker t)
        {
            if (ExistInRegisters(t) == false &&
                ExistInSSERegisters(t) == false &&
                ExistInStack(t) == true)
                return;

            var register = GetRegister(t);
            var area = this.AllocateAreaInStack(t, t.Alignment);
            EmitMovRegisterToMemory(register, RegisterName.rbp, area.StackOffset, area.Size, NativeSymbolTable);
        }

        public void Block(RegisterName register)
        {
            foreach (var e in this.RegisterMap)
                if (X86_64.X86_64_Model.Equals(e.Register, register))
                {
                    e.IsBlocked = true;
                    return;
                }

            foreach (var e in this.SSERegisterMap)
                if (e.Register == register)
                {
                    e.IsBlocked = true;
                    return;
                }
        }
        public void Unblock(RegisterName register)
        {
            foreach (var e in this.RegisterMap)
                if (X86_64_Model.Equals(e.Register, register))
                {
                    e.IsBlocked = false;
                    return;
                }

            foreach (var e in this.SSERegisterMap)
                if (e.Register == register)
                {
                    e.IsBlocked = false;
                    return;
                }
        }

        public void Block(MemoryIdTracker t)
        {
            if (ExistInRegisters(t) || ExistInSSERegisters(t))
                Block(GetRegister(t));
            else
                throw new CompileException();
        }
        public void Unblock(MemoryIdTracker t)
        {
            if (ExistInRegisters(t) || ExistInSSERegisters(t))
                this.Unblock(GetRegister(t));
            else
                throw new CompileException();
        }

        public void MoveToStack(MemoryIdTracker data, VariableIdTracker place)
        {
            MoveToAnyRegister(data);
            var register = GetRegister(data);

            generateMovRegisterToAnyContext(place, register, this.NativeSymbolTable);
            SetStackArea(data, place);
        }

        public void SetStackArea(MemoryIdTracker data, MemoryIdTracker place)
        {
            if (place is VariableIdTracker)
            {
                return;
            }
            else
            {
                var area = GetStackArea(data);
                area.ContainedData = data;
            }
        }

        public MemoryAreaElement GetStackArea(MemoryIdTracker tracker)
        {
            foreach (var e in StackModel)
                if (e.ContainedData == tracker)
                    return e;

            var mRef = this.NativeSymbolTable.GetMethodContextRef();

            if (mRef is null)
                return null;
            else
                return mRef.Context.MemoryAllocator.GetStackArea(tracker);
        }
        /***   memory-tracker methods   ***/

        public RegisterName GetRegister(MemoryIdTracker t)
        {
            if (ExistInRegisters(t))
            {
                if (t.IsRbp)
                    return RegisterName.rbp;


                foreach (RegisterMapElement e in RegisterMap)
                {
                    if (t == e.ContainedData)
                        return e.Register;
                }
            }

            throw new CompileException();
        }
        public bool ExistInRegisters(MemoryIdTracker t)
        {
            var regMap = this.RegisterMap;

            foreach (RegisterMapElement e in regMap)
            {
                if (t == e.ContainedData)
                    return true;
            }

            if (t.IsRbp)
                return true;

            return false;
        }

        public bool ExistInSSERegisters(MemoryIdTracker t)
        {
            foreach (RegisterMapElement e in this.SSERegisterMap)
            {
                if (e.ContainedData == t)
                    return true;
            }
            return false;
        }
        public bool ExistInStack(MemoryIdTracker t)
        {
            foreach (MemoryAreaElement area in StackModel)
            {
                if (t == area.ContainedData)
                    return true;
            }
            return false;
        }

        public void Free(MemoryIdTracker t)
        {
            FreeFromRegister(t);
            FreeFromStack(t);
        }

        public void FreeFromRegister(MemoryIdTracker t)
        {
            SetRegisterFree(t);
        }

        public void FreeFromStack(MemoryIdTracker t)
        {
            if (t is VariableIdTracker)
                return;

            foreach (var e in this.StackModel)
                if (t == e.ContainedData)
                    e.Free();
        }

        public int GetRbp_Offset(MemoryIdTracker t)
        {
            foreach (var e in this.StackModel)
                if (t == e.ContainedData)
                    return e.Rbp_Offset;

            throw new CompileException();
        }

        public int GetStackOffset(MemoryIdTracker t)
        {
            foreach (var e in this.StackModel)
                if (e.ContainedData == t)
                    return e.StackOffset;

            throw new CompileException();
        }
    }
}
