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

        public static void Read (PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { StringValue = Console.ReadLine() });
        }

        public static void ReadInt(PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { IntValue = int.Parse(Console.ReadLine()) });
        }

        public static void ReadDouble(PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { DoubleValue = double.Parse(Console.ReadLine())});
        }

        private static void StrToInt(PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { IntValue = int.Parse(args[0].StringValue) });
        }

        private static void IntToDouble(PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { DoubleValue = (double)args[0].IntValue });
        }

        private static void StrToDouble(PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { DoubleValue = double.Parse(args[0].StringValue) });
        }

        public static void DoubleToInt(PeakObject[] args, RuntimeThread rt)
        {
            rt.PushOnStack(new PeakObject() { IntValue = (int)args[0].DoubleValue });
        }
    }
}
