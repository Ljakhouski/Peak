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
        public string ExecuteResult(string inputFileName)
        {
            var dbs = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            var peakFolder = dbs.Parent.Parent.Parent.Parent;
            var testsPath = peakFolder.FullName + @"\tests\";
            var compilerPath = peakFolder.FullName + @"\bin\Compiler\PeakC.exe";

            ProcessStartInfo compilerProcess = new ProcessStartInfo();
            compilerProcess.CreateNoWindow = true;
            compilerProcess.WindowStyle = ProcessWindowStyle.Hidden;
            compilerProcess.UseShellExecute = false;
            compilerProcess.WorkingDirectory = Path.GetDirectoryName(compilerPath);
            compilerProcess.FileName = compilerPath;
            compilerProcess.Arguments = testsPath + inputFileName;
            //compilerProcess.RedirectStandardOutput = true;
            //compilerProcess.RedirectStandardError = true;

            if (Directory.Exists("Output") == false)
                Directory.CreateDirectory("Output");

            //compilerProcess.
            //Process.Start(compilerProcess);
            var proc = new Process();
            proc.StartInfo = compilerProcess;
            proc.Start();

            /*****/

            ProcessStartInfo testProc = new ProcessStartInfo();
            testProc.CreateNoWindow = false;
            //testProc.WindowStyle = ProcessWindowStyle.Hidden;
            testProc.UseShellExecute = false;
            testProc.FileName = testsPath+ Path.GetFileNameWithoutExtension(inputFileName) +".exe";
            testProc.WorkingDirectory = "C:\\source\\Peak\\tests";
            testProc.RedirectStandardOutput = true;
            testProc.RedirectStandardError = true;
            
            if (Directory.Exists("Output") == false)
                Directory.CreateDirectory("Output");

            //compilerProcess.
            //Process.Start(compilerProcess);
            var exec = new Process();
            exec.StartInfo = testProc;
            //exec.EnableRaisingEvents = true;
            exec.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            exec.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
            exec.Start();
            exec.BeginOutputReadLine();
            exec.BeginErrorReadLine();
            exec.WaitForExit();
            //exec.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            /*exec.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                output = e.Data;
            });
            exec.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                output = e.Data;
            });*/

            //i am commented this
            //exec.Start();
            //exec.BeginOutputReadLine();
            //exec.BeginErrorReadLine();
            //
            //exec.WaitForExit();
            //exec.Dispose();
            return output;
        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
                output = outLine.Data;
        }
    }
}