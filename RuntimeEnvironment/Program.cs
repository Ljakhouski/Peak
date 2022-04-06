using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RuntimeEnvironment
{
    class Data { public string S = ""; int type = 0; public Data(int y) { S = y.ToString(); } public Data() { } }
    class Program
    {
        static void Main(string[] args)
        {
            /*
            for (int i = 0; i< args.Length; i++)
            {
                if (args[i] == "-f")
            }*/

            RuntimeModule.RuntimeModule module;

            FileStream fs = null;
            try
            {
                fs = new FileStream(Directory.GetCurrentDirectory() /*Assembly.GetExecutingAssembly().Location*/ + "/module.pem", FileMode.Open);

                BinaryFormatter formatter = new BinaryFormatter();

                module = (RuntimeModule.RuntimeModule)formatter.Deserialize(fs);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to load runtime module. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var thread = new RuntimeThread(module, ConstantBuilder.GetConstant(module));
            thread.Execute(module.Methods[0]);

            stopWatch.Stop();
            Console.WriteLine("execute time: " + stopWatch.ElapsedMilliseconds+" ms");
        }

    }
}

