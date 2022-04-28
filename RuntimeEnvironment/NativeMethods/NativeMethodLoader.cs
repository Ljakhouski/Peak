using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.NativeMethods
{
    public static partial class NativeMethods
    {
        public delegate void NativeMethodDelegate(PeakObject[] args, RuntimeThread rt);
        public static Dictionary<string, NativeMethodDelegate> GetNativeMethods()
        {
            var methods = new Dictionary<string, NativeMethodDelegate>()
            {
                { "print_i", PrintInteger},
                { "print_d", PrintDouble},
                { "print_s", PrintString},
                { "read", Read},
                { "readInt", ReadInt},
                { "readDouble", ReadDouble},
                { "double_to_int", DoubleToInt},
                { "str_to_double", StrToDouble},
                { "int_to_double", IntToDouble},
                { "str_to_int", StrToInt},
            };

            return methods;
        }

       
    }
}
