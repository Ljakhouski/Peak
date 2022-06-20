using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
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
        R15

    }
    class AsmInstruction
    {
        public InstructionName InstructionName { get; set; }

        public bool IsLabelOnly { get; set; } = false; // example: "  main:   ; entry point  "
        public string Comment { get; set; }  

        public Operand FistOperand { get; set; }
        public Operand SecondOperand { get; set; }
        public Operand ThirdOperand { get; set; }

    }

    class Operand
    {
        public bool IsGettingAddress { get; set; } = false; // (for FASM) if true: [ RDX ]   if false:  RDX 
        public bool IsLabelOperand { get; set; } = false;
        public RegisterName RegisterName { get; set; }
        public string Label { get; set; }
    }
    class AsmModel
    {

    }
}
