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
            };

            return methods;
        }
    }
}
