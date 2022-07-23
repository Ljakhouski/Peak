using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    public enum InstructionName
    {
        mov,
        push,
        pop,
        
        add,
        sub,
        mul,
        div,

        jmp,
        call,
        ret,
        label,
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
        rsp,
        rbp,

        rax,
        rbx,
        rcx,
        rdx,
        rsi,
        rdi,

        r8,
        r9,
        r10,
        r11,
        r12,
        r13,
        r14,
        r15,

        xmm0,
        xmm1,
        xmm2,
        xmm3,
        xmm4,
        xmm5,
        xmm6,
        xmm7,
        NONE
    }
    class AsmInstruction
    {
        public string content { get; set; }
        public string comment { get; set; }
    }
  
    class AsmMethod
    {
        public string MethodName { get; set; }
        public List<AsmInstruction> Code { get; set; } = new List<AsmInstruction>();

        public void Emit(string instruction)
        {
            this.Code.Add(new AsmInstruction() { content = instruction});
        }

        public void Emit(string instruction, string comment)
        {
            this.Code.Add(new AsmInstruction() { content = instruction, comment = comment });
        }
    }
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

            foreach(AsmMethod method in Code)
            {
                output += "\n\n\n";
                
                foreach(AsmInstruction instruction in method.Code)
                {
                    string line = getInstructionListing(instruction) + '\n';
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

            output += instruction.content;

            if (instruction.comment != null && instruction.comment.Length != 0)
                output += "    ;"+instruction.comment;
            return output;
        }
    }
}
