using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment
{
    static class ConstantBuilder
    {
        public static PeakObject[] GetConstant(RuntimeModule.RuntimeModule module)
        {
            var constants = new PeakObject[module.Constant.Length];
            for (int i = 0; i < module.Constant.Length; i++)
            {
                constants[i] = new PeakObject()
                {
                    BoolValue = module.Constant[i].BoolValue,
                    IntValue = module.Constant[i].IntValue,
                    DoubleValue = module.Constant[i].DoubleValue,
                    StringValue = module.Constant[i].StrValue,
                };
            }

            return constants;
        }
    }
}
