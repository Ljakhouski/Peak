using System.Diagnostics;
using System.Reflection;

namespace Peak.UnitTests
{
    [TestClass]
    public class PeakCTest
    {
        [TestMethod]
        public void Test1()
        {
            ExecuteResult("test1.p");
            Assert.AreEqual(output, "3");
        }
        string output = "";
        public string ExecuteResult(string fileName)
        {
            var dbs = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            ProcessStartInfo compilerProcess = new ProcessStartInfo();
            compilerProcess.CreateNoWindow = true;
            compilerProcess.WindowStyle = ProcessWindowStyle.Hidden;
            compilerProcess.UseShellExecute = false;
            compilerProcess.WorkingDirectory = "\\Compiler";
            compilerProcess.FileName = "Compiler/PeakC.exe";
            compilerProcess.Arguments = $"..\\..\\{fileName}";
            compilerProcess.RedirectStandardOutput = true;
            compilerProcess.RedirectStandardError = true;

            if (Directory.Exists("Output") == false)
                Directory.CreateDirectory("Output");

            //compilerProcess.
            //Process.Start(compilerProcess);
            var proc = new Process();
            proc.StartInfo = compilerProcess;


            /*****/

            ProcessStartInfo testProc = new ProcessStartInfo();
            testProc.CreateNoWindow = true;
            testProc.WindowStyle = ProcessWindowStyle.Hidden;
            testProc.UseShellExecute = false;
            testProc.FileName = "Compiler/PeakC.exe";
            testProc.RedirectStandardOutput = true;
            testProc.RedirectStandardError = true;

            if (Directory.Exists("Output") == false)
                Directory.CreateDirectory("Output");

            //compilerProcess.
            //Process.Start(compilerProcess);
            var exec = new Process();
            exec.StartInfo = testProc;
            exec.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            exec.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            exec.Start();
            return output;
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            output = outLine.Data;
        }
    }
}