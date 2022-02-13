using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Peak.CodeGeneration;
using Peak.PeakC.Parser;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.PeakC
{
    class Program
    {
        static string inputPath;
        static string outputPath = "";
        static string name = "";
        static void Main(string[] args)
        {
            if (args.Length > 0)
                inputPath = args[0];
            else
            {
                Console.WriteLine("missing .p file path");
                return;
            }
            try
            {
                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-o")
                    {
                        outputPath = args[i + 1];
                        i++;
                        continue;
                    }
                    else if (args[i] == "-n")
                    {
                        name = args[i + 1];
                        i++;
                    }

                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("missing argument");
                return;
            }

            Stopwatch timer = new Stopwatch();
            timer.Start();

            NonterminalPreority.MakePriorityList();

            RuntimeModule module;
            try
            {
                Parser.Parser pars = new Parser.Parser();
                Node n = pars.GetNode(inputPath);

                module = new ByteCodeGenerator().GetProgramRuntimeModule((ProgramNode)n);
            }
            catch (CompileException e) { return; }

            module.ModuleName = module.ModuleName == null ? "module" : module.ModuleName;
            name = name == "" ? module.ModuleName : name;
            BinaryFormatter formatter = new BinaryFormatter();

            //string path = "module.pem";
            try
            {
                using (FileStream fs = new FileStream(outputPath + name + ".pem", FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, module);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            timer.Stop();
            Console.WriteLine("Compiled time: " + timer.ElapsedMilliseconds + " ms");
            //Console.ReadKey();
        }
    }
}

