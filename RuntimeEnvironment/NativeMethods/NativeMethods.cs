using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.NativeMethods
{
    public static partial class NativeMethods
    {
        public static void PrintInteger(PeakObject[] args, RuntimeThread rt)
        {
            Console.WriteLine(args[0].IntValue);
        }
        public static void PrintDouble(PeakObject[] args, RuntimeThread rt)
        {
            Console.WriteLine(args[0].DoubleValue);
        }
        public static void PrintString(PeakObject[] args, RuntimeThread rt)
        {
            Console.WriteLine(args[0].StringValue);
        }

        
    }
}
