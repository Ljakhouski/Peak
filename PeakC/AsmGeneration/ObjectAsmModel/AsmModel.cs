using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration.ObjectAsmModel
{
    public enum InstructionName
    {
        Mov,
        Push,
        Pop,

        Add,
        Sub,
        Mul,
        Div,

        Jmp,
        Call,
        Ret,
        LABEL,
    }

    public enum DataSize
    {
        None,
        Byte,
        Dword,
        QWord,
        Dqword,
        Word
    }
    public enum RegisterName
    {
        RSP,
        RBP,

        RAX,
        RBX,
        RCX,
        RDX,
        RSI,
        RDI,

        R8,
        R9,
        R10,
        R11,
        R12,
        R13,
        R14,
        R15,

        XMM0,
        XMM1,
        XMM2,
        XMM3,
        XMM4,
        XMM5,
        XMM6,
        XMM7,
        NONE
    }
    [Obsolete]
    class AsmInstruction
    {
        public InstructionName InstructionName { get; set; }

        public string Comment { get; set; }
        public string Label { get; set; }

        public Operand FirstOperand { get; set; }
        public Operand SecondOperand { get; set; }
        public Operand ThirdOperand { get; set; }

    }
    [Obsolete]
    class Operand
    {
        public bool IsGettingAddress { get; set; } = false; // (for FASM) if true: [ RDX ]   if false:  RDX 
        public bool DataSizeExist { get; set; } = false;
        public int Offset { get; set; } = 0;
        public bool IsLabelOperand { get; set; } = false;
        public RegisterName RegisterName { get; set; } = RegisterName.NONE;
        public DataSize Size { get; set; } = DataSize.None;
        public string Label { get; set; }
    }
    [Obsolete]
    class AsmMethod
    {
        public string MethodName { get; set; }
        public List<AsmInstruction> Code { get; set; } = new List<AsmInstruction>();

        public void Emit(InstructionName name, RegisterName r1)
        {
            this.Code.Add(new AsmInstruction()
            {
                InstructionName = name,
                FirstOperand = new Operand() { IsLabelOperand = false, RegisterName = r1 }
            });
        }

        public void Emit(InstructionName name, RegisterName r1, RegisterName r2)
        {
            this.Code.Add(new AsmInstruction()
            {
                InstructionName = name,
                FirstOperand = new Operand() { IsLabelOperand = false, RegisterName = r1 },
                SecondOperand = new Operand() { IsLabelOperand = false, RegisterName = r2 }
            });
        }

        public void Emit(InstructionName name, RegisterName r1, RegisterName r2, RegisterName r3)
        {
            this.Code.Add(new AsmInstruction()
            {
                InstructionName = name,
                FirstOperand = new Operand() { IsLabelOperand = false, RegisterName = r1 },
                SecondOperand = new Operand() { IsLabelOperand = false, RegisterName = r2 },
                ThirdOperand = new Operand() { IsLabelOperand = false, RegisterName = r3 },
            });
        }

        public void Emit(InstructionName name, RegisterName r1, Operand op)
        {
            this.Code.Add(new AsmInstruction()
            {
                InstructionName = name,
                FirstOperand = new Operand()
                {
                    RegisterName = r1
                },
                SecondOperand = op
            });
        }

        public void Emit(InstructionName name, string label, bool inSquareParents = false)
        {
            this.Code.Add(new AsmInstruction()
            {
                InstructionName = name,
                FirstOperand = new Operand()
                {
                    IsGettingAddress = inSquareParents,
                    IsLabelOperand = true,
                    Label = label
                }
            });
        }

        public void Emit(InstructionName name)
        {
            this.Code.Add(new AsmInstruction() { InstructionName = name });
        }

        public void Emit(AsmInstruction instruction)
        {
            this.Code.Add(instruction);
        }

        public void Emit(InstructionName name, Operand op, RegisterName r)
        {
            this.Code.Add(new AsmInstruction()
            {
                InstructionName = name,
                FirstOperand = op,
                SecondOperand = new Operand() { RegisterName = r }
            });
        }

    }
    [Obsolete]
    class AsmModel
    {
        public const int ByteInWord = 8; // for x64 mode 
        public List<string> Head { get; set; } = new List<string>();
        public List<string> RData { get; set; } = new List<string>();
        public List<string> IData { get; set; } = new List<string>();
        public List<AsmMethod> Code { get; set; } = new List<AsmMethod>();

        public string GetFasmListing()
        {
            string output = "format PE64 Console \n entry start  \n";

            foreach (string heads in Head)
                output += heads + '\n';

            output += "\nsection '.text' code readable executable  \n";

            // code adding

            foreach (AsmMethod method in Code)
            {
                output += "\n\n\n";

                foreach (AsmInstruction instruction in method.Code)
                {
                    string line = "   " + getInstructionListing(instruction) + '\n';
                    output += line;
                }
            }

            output += "\nsection '.rdata' data readable \n";

            foreach (string S in RData)
                output += S;

            output += "\nsection '.idata' data readable import \n";
            foreach (string S in IData)
                output += S;

            return output;
        }

        private string getInstructionListing(AsmInstruction instruction)
        {
            string output = "    ";

            if (instruction.InstructionName == InstructionName.LABEL)
                return instruction.Label;
            output += instruction.InstructionName.ToString();

            if (instruction.FirstOperand is null == false)
                output += getOperandListing(instruction.FirstOperand);
            if (instruction.SecondOperand is null == false)
                output += ", " + getOperandListing(instruction.SecondOperand);
            if (instruction.ThirdOperand is null == false)
                output += ", " + getOperandListing(instruction.ThirdOperand);


            if (instruction.Comment != null)
                output += "       ; " + instruction.Comment;

            string getOperandListing(Operand o)
            {
                string operandOutput = " ";

                if (o.Size != DataSize.None) // byte/word/dword/qword
                {
                    operandOutput += o.Size.ToString();
                }

                if (o.IsGettingAddress)
                {
                    operandOutput += " [";
                }

                if (o.IsLabelOperand)
                    operandOutput += o.Label;

                if (o.RegisterName != RegisterName.NONE)
                    operandOutput += o.RegisterName.ToString();

                if (o.Size != 0) // [rbp +- 8]
                {
                    if (o.Size > 0)
                    {
                        operandOutput += "+";
                    }
                    operandOutput += o.Size.ToString();
                }

                if (o.IsGettingAddress)
                {
                    operandOutput += "]";
                }

                return operandOutput;
            }

            return output;
        }
    }
}
