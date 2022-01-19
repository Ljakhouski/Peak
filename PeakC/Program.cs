using System;
using Peak.PeakC.Parser;

namespace Peak.PeakC
{
    class Program
    {
        static void Main (string[] args)
        {
            //Test:

            Lexer l = new Lexer(args[0]);
            /*while (!l.EndOfFile())
            {
                var t = l.GetToken();
                Console.WriteLine(t.Content+"   :"+t.Type.ToString());
            }*/

            Preprocessor preproc = new Preprocessor(l);
            while (preproc.NextTokenExist())
                Console.WriteLine("token: "+preproc.GetNextToken().Content);

            Parser.Parser pars = new Parser.Parser();
            Node n = pars.GetNode(args[0]);
        }
    }
}

