using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;
namespace Peak.CodeGeneration
{
    class GenerationResult
    {
        public bool Nothing { get; set; }
        public SymbolType ExprResult { get; set; }

        public TableElement NameResult { get; set; } // only for analysis data-contains in name from name-table

        public ByteCodeResult GeneratedByteCode { get; set; } = new ByteCodeResult();
    }

    class ByteCodeResult
    {
        public List<Instruction> ByteCode = new List<Instruction>();

        public void AddByteCode(InstructionName name, int operand)
        {
            this.ByteCode.Add(new Instruction()
            {
                Name = name,
                Operands = new int[1] { operand }
            });
        }

        public void AddByteCode(InstructionName name, int op1, int op2)
        {
            this.ByteCode.Add(new Instruction()
            {
                Name = name,
                Operands = new int[2] { op1, op2 }
            });
        }

        public void AddByteCode(InstructionName name)
        {
            this.ByteCode.Add(new Instruction()
            {
                Name = name
            });
        }
        public void AddByteCode(GenerationResult result)
        {
            this.ByteCode.AddRange(result.GeneratedByteCode.ByteCode);
        }

        public void AddByteCode(ByteCodeResult result)
        {
            this.ByteCode.AddRange(result.ByteCode);
        }

    }
}