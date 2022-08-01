using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    class GenResult
    {
        //public Operand Operand { get; set; }

        public SemanticType ResultType { get; set; }
        public MemoryDataId ReturnDataId { get; set; }

        public virtual void PushOnStack(SymbolTable st)
        {
            st.MemoryAllocator.MoveToRegister(ReturnDataId);
            if (this.ReturnDataId.IsSSE_Element)
            {
                if (this.ReturnDataId.ExistInSSERegisters)
                    st.Emit($"movsd [rbp +-...], ");
                else
                    st.Emit($"push {ReturnDataId.Register}");
            }
            else
            {
                st.Emit($"push {ReturnDataId.Register}");
            }
        }
    }

    class EmptyGenResult : GenResult
    {

    }
    class ConstantResult : GenResult
    {
        /*
        public int IntValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }*/
        public Token ConstValue { get; set; }

        public override void PushOnStack(SymbolTable st)
        {
            if (this.ResultType.Type == Type.Bool)
            {
                char ch = this.ConstValue.Content == "true"? '1' : '0';
                st.Emit($"push {ch}");
            }
            else if (this.ResultType.Type == Type.Int)
            {
                st.Emit($"push {this.ConstValue.Content}");
            }
            else if (this.ResultType.Type == Type.Double)
            {
                throw new CompileException("SSE const not implemented");
            }
        }

        public GenResult MoveToRegister(SymbolTable st)
        {
            var result = ResultType;

            if (result.Type == Type.Int || result.Type == Type.Bool)
            {
                var reg = st.MemoryAllocator.GetFreeRegister();
                var labelContent = ConstValue.Content;
                var size = result.Type == Type.Int ? 8 : 1;

                if (labelContent == "true")
                    labelContent = "1";
                else if (labelContent == "false")
                    labelContent = "0";

                st.Emit(string.Format("mov {0}, {1}", reg.ToString(), labelContent));

                /*
                st.MethodCode.Emit(
                        InstructionName.Mov,
                        reg,
                        new Operand()
                        {
                            IsLabelOperand = true,
                            Label = labelContent
                        }
                    );*/

                var id = new MemoryDataId(st) { Size = size };
                st.MemoryAllocator.SetIdToFreeRegister(id, reg);
                
                return new GenResult() { ResultType = result, ReturnDataId = id };
            }
            else
                throw new CompileException("other types not support now");
        }
    }
}
