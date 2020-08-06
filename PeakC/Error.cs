using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC
{
    class Error
    {
        static public void ErrMessage(Token t, string message)
        {
            Console.WriteLine("Error in file \"{0}\" in line \"{1}\" (token: {3}): {2}.", t.FilePosition, t.LinePosition, message, t.Content);
            Console.ReadKey();
            Environment.Exit(0);
        }

        static public void WarningMessage(Token t, string message)
        {
            Console.WriteLine("Warning in file \"{0}\" in line \"{1}\" (token: {3}): {2}.).", t.FilePosition, t.LinePosition, message, t.Content);
        }
        
        public static void FileNotFoundErrMessage(string s)
        {
            Console.WriteLine("Error: file " + s + " not found");
            Console.ReadKey();
            Environment.Exit(0);
        }

    }


}
