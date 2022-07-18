using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class Constant
    {
        public static GenResult Generate(ConstValueNode node, SymbolTable st)
        {
            return new ConstantResult()
            {
                ConstValue = node.Value,
                ResultType = new SymbolType(node),
                ReturnDataId = null
            };
        }

        public static GenResult GenerateConstInRegister(ConstantResult constant, SymbolTable st)
        {
            var result = constant.ResultType;

            if (result.Type == Type.Int || result.Type == Type.Bool)
            {
                var reg = st.MemoryAllocator.GetFreeRegister();
                var labelContent = constant.ConstValue.Content;
                var size = result.Type == Type.Int ? 8 : 1;

                if (labelContent == "true")
                    labelContent = "1";
                else if (labelContent == "false")
                    labelContent = "0";

                st.MethodCode.Emit(
                        InstructionName.Mov,
                        reg,
                        new Operand()
                        {
                            IsLabelOperand = true,
                            Label = labelContent
                        }
                    );

                var id = new MemoryDataId(st) { Size = size };
                st.MemoryAllocator.SetIdToFreeRegister(id, reg);

                return new GenResult() { ResultType = result, ReturnDataId = id };
            }
            else
                throw new CompileException("other types not support now");
            
        }
        
    }
}
