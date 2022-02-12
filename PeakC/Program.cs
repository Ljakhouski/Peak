using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Peak.CodeGeneration;
using Peak.PeakC.Parser;

namespace Peak.PeakC
{
    class Program
    {
        static void Main (string[] args)
        {
            NonterminalPreority.MakePriorityList();
            //Test:
            
            Lexer l = new Lexer(args[0]);
            /*while (!l.EndOfFile())
            {
                var t = l.GetToken();
                Console.WriteLine(t.Content+"   :"+t.Type.ToString());
            }

            Preprocessor preproc = new Preprocessor(l);
            while (preproc.NextTokenExist())
            {
                var t = preproc.GetNextToken();
                Console.WriteLine("token: "+t.Content + "  line: "+t.Line+ " pos: "+t.Position);
            }
                */

            Parser.Parser pars = new Parser.Parser();
            Node n = pars.GetNode(args[0]);
            var codeGen = new ByteCodeGenerator();
            var module = codeGen.GetProgramRuntimeModule((ProgramNode)n);

            BinaryFormatter formatter = new BinaryFormatter();

            string path = "module.pem";
            if (args.Length > 1)
                path = args[1] + path;


            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                // сериализуем весь массив people
                formatter.Serialize(fs, module);
            }
        }
    }
}

