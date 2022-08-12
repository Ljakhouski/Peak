using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class Constant
    {
        public static GenResult Generate(ConstValueNode node, SymbolTable st)
        {
            return new ConstantResult()
            {
                ConstValue = node.Value,
                ResultType = new SemanticType(node),
                ReturnDataId = null
            };
        }
        [Obsolete]
        public static GenResult GenerateConstInRegister(ConstantResult constant, SymbolTable st)
        {
            var result = constant.ResultType;

            if (result.Type == Type.Int || result.Type == Type.Bool)
            {
                var reg = st.MemoryAllocator.GetFreeRegister();

                var labelContent = constant.ConstValue.Content;
                var size = result.Type == Type.Int ? 8 : 1;
                reg = X86_64_Model.CastRegister(reg, size);
                var opSize = X86_64_Model.GetSizeName(size);

                if (labelContent == "true")
                    labelContent = "1";
                else if (labelContent == "false")
                    labelContent = "0";

                st.Emit($"mov {reg}, {opSize} {labelContent}");


                var id = new MemoryIdTracker(st, size);
                st.MemoryAllocator.SetIdToFreeRegister(id, reg);

                return new GenResult() { ResultType = result, ReturnDataId = id };
            }
            else
                throw new CompileException("other types not support now");

        }

    }
}
