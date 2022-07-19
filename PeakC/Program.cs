using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Peak.AsmGeneration;
using Peak.CodeGeneration;
using Peak.PeakC.Parser;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.PeakC
{
    class Program
    {
        public static string inputPath;
        public static string outputPath = "";
        public static string name = "";
        public static bool   showInfo = false;
        public static bool   genByteCodeForInterpreter = false;
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
                    else if (args[i] == "-i")
                    {
                        showInfo = true;
                        i++;
                    }
                    else if (args[i] == "-bc")
                    {
                        genByteCodeForInterpreter = true;
                        i++;
                    }
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine("missing argument");
                return;
            }

            if (showInfo) Console.WriteLine("compiling " + inputPath + "  |  current directory: " + Directory.GetCurrentDirectory());

            Stopwatch timer = new Stopwatch();
            timer.Start();

            NonterminalPreority.MakePriorityList();

            if (genByteCodeForInterpreter)
                GenForInterpreter();
            else
                Gen_x86_64_PE();
            

            timer.Stop();
            Console.WriteLine("Compiled time: " + timer.ElapsedMilliseconds + " ms");
            //Console.ReadKey();
        }

        public static void Gen_x86_64_PE()
        {
            var pars = new Parser.Parser();
            var n = pars.GetNode(inputPath);
            try
            {
                var assembly = AsmGeneration.CodeGeneration.GetAsmAssembly(n);
                var listing = assembly.GetFasmListing();

                using (StreamWriter writer = new StreamWriter(outputPath))
                {
                    writer.WriteLine(listing);
                }
            }
            catch(CompileException e)
            {
                Console.WriteLine("Compile error: " + e.Message); return;
            }
            catch(DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public static void GenForInterpreter()
        {

            RuntimeModule module = null;
            try
            {
                var pars = new Parser.Parser();
                var n = pars.GetNode(inputPath);

                if (genByteCodeForInterpreter)

                    module = new ByteCodeGenerator().GetProgramRuntimeModule(n);


            }
            catch (CompileException e) { Console.WriteLine("Compile error: " + e.Message); return; }
            /*catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }*/

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
        }
    }
}

