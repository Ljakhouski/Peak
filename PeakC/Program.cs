using System;

namespace Peak.PeakC
{
    class Program
    {
        static void Main (string[] args)
        {
            //Test:
            Lexer l = new Lexer(args[0]);
            while (!l.EndOfFile())
            {
                var t = l.GetToken();
                Console.WriteLine(t.Content+"   :"+t.Type.ToString());
            }
                

        }
    }
}

