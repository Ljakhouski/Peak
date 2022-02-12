﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC
{
    class Error
    {
        static public void ErrMessage(Token t, string message)
        {
            if (t == null)
                t = new Token() { };
            Console.WriteLine("Error in file \"{0}\" in line \"{1}\" (token: {3}): {2}.", t.File, t.Line, message, t.Content);
            Console.ReadKey();
            Environment.Exit(0);
        }
        static public void UnknowTokenErrMessage(Token t)
        {
            Console.WriteLine("Error in file \"{0}\" in line \"{1}\" unknow token \"{2}\"", t.File, t.Line, t.Content);
            Console.ReadKey();
            Environment.Exit(0);
        }

        static public void WarningMessage(Token t, string message)
        {
            Console.WriteLine("Warning in file \"{0}\" in line \"{1}\" (token: {3}): {2}.).", t.File, t.Line, message, t.Content);
        }
        
        public static void FileNotFoundErrMessage(string fileName)
        {
            Console.WriteLine("Error: file " + fileName + " not found");
            Console.ReadKey();
            Environment.Exit(0);
        }

        public static void NameNotExistError(IdentifierNode id)
        {
            Console.WriteLine("name \"" + id.Id.Content + "\" does not exist");
            Console.ReadKey();
            Environment.Exit(0);
        }
        public static void NameNotExistError(Token name)
        {
            Console.WriteLine("name \"" + name.Content + "\" does not exist");
            Console.ReadKey();
            Environment.Exit(0);
        }

    }


}
