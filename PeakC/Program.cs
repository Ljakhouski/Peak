using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using Peak.AsmGeneration;
using Peak.CodeGeneration;
using Peak.PeakC.Parser;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.PeakC
{
    class Program
    {
        public static string inputPath = "";
        public static string outputPath = "";
        public static string name = "";
        public static bool   showInfo = false;
        public static bool   genByteCodeForInterpreter = false;
        public static bool   printAsmtListing = false;
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
                    else if (args[i] == "-l")
                    {
                        printAsmtListing = true;
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

            if (showInfo) Console.WriteLine("compiling " + inputPath + "  |  output path: " + outputPath);

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
                if (printAsmtListing)
                    Console.Write("\n\n        *** FASM listing: ***\n\n\n" + listing + "\n ______________________________\n");

                string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                using (StreamWriter writer = new StreamWriter(currentDir + "\\fasm\\output.ASM", false))
                {
                    writer.WriteLine(listing);
                }

                runFasm();
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

        private static void runFasm()
        {            
            var arg1 = "output.ASM";
            var outputPEName = "";
            var outputPEPath = "";
            if (outputPath == "")
            {
                outputPEName = getName(inputPath) + ".exe"; 
                outputPEPath = Path.GetDirectoryName(Path.GetFullPath(inputPath));
            }
            else
            {
                outputPEName = getName(outputPath) + ".exe";
                outputPEPath = Path.GetDirectoryName(Path.GetFullPath(outputPath));
            }            

            string fullFasmDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\fasm";
            Console.WriteLine(fullFasmDirectory);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.WorkingDirectory = fullFasmDirectory;
            startInfo.FileName = "fasm\\FASM.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = arg1 + " " + outputPEName;
           
            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    var reader = exeProcess.StandardOutput;
                    var output = reader.ReadToEnd();
                    exeProcess.WaitForExit();
                }

                File.Copy(fullFasmDirectory + '\\' + outputPEName, outputPEPath + '\\' + outputPEName, true);
            }
            catch(Exception e)
            {
                Console.WriteLine(".exe-PE build faild: " + e.Message);
            }
        }

        private static string getName(string fullName)
        {
            for (int i = fullName.Length - 1; i>=0; i--)
            {
                if (fullName[i] == '.')
                {
                    string name = "";
                    for (int j = i-1; j>=0; j--)
                    {
                        if (fullName[j] == '/' || fullName[j] == '\\')
                            return name;
                        else
                        name = fullName[j] + name;
                    }
                    return name;
                }
            }
            throw new CompileException("separating error");
        }
        /*
        private static string getPath(string input)
        {
            for (int i = input.Length - 1; i >=0; i--)
                if (input[i]=='\\' || input[i]=='/')
                    return input.Substring(0, i + 1);
            
            return input;
        }*/
        private static string getStandartOutputDir()
        {
            var path = Path.GetFullPath(inputPath);

            while ( path[path.Length - 1] != '\\' && path[path.Length - 1] != '/')
                path = path.Remove(path.Length - 1);

            path += getName(inputPath) + ".exe";

            return path;
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

