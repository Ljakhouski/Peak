using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace IDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            makeNewTab("new.p"/*, Directory.GetCurrentDirectory()+"\\"*/);
            insertBaseText();
            this.MessageTextBox.AppendText("IDE started " + DateTime.Now.ToString() + "\n");
        }

        private void loadHighlighter(TextEditor editor)
        {
            XshdSyntaxDefinition xshd;
            using (XmlTextReader reader = new XmlTextReader("PeakCodeHighlighter.xshd"))
            {
                xshd = HighlightingLoader.LoadXshd(reader);
            }
            using (XmlTextWriter writer = new XmlTextWriter("PeakCodeHighlighter.xshd", System.Text.Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                new SaveXshdVisitor(writer).WriteDefinition(xshd);
            }
            //codeEditor.SyntaxHighlighting = xshd;
            editor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
        }

        private void makeNewTab(string fileName, string path = "")
        {
            var editor = new TextEditor();
            editor.ShowLineNumbers = true;
            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 16;
            //editor.Background = new SolidColorBrush() { Color = Color.FromArgb(10, 10, 20, 20) };
            loadHighlighter(editor);

            var tabHeader = new CustomTabHeader(mainTabControl.Items.Count, mainTabControl, editor, this, fileName, path);


            mainTabControl.Items.Add(new TabItem() { Content = editor, Header = /*"new.p"*/  tabHeader });
            mainTabControl.SelectedIndex = mainTabControl.Items.Count - 1;
        }

        private void insertBaseText()
        {
            (((TabItem)mainTabControl.Items[mainTabControl.Items.Count - 1]).Content as TextEditor).Text
            = "load \"std.p\";\nprint(\"Hello World\");";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            makeNewTab("unknow.p");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var header = (mainTabControl.SelectedItem as TabItem).Header as CustomTabHeader;
            header.SaveFile();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Filter = "Peak project (*.p)|*.p|All files (*.*)|*.*" };
            if (dialog.ShowDialog() == true)
            {
                string[] splitResult = dialog.FileName.Split('\\');
                string[] pathArray = (string[])splitResult.Clone();
                Array.Resize(ref pathArray, pathArray.Length - 1);

                string path = "";
                foreach (string S in pathArray) { path += S + '\\'; }
                makeNewTab(splitResult[splitResult.Length - 1], path);
            }

        }

        private void viewDisasm(object sender, RoutedEventArgs e)
        {
            var viewer = new DisAsmViewer();
            viewer.MakeDisasbInfo("Output/module.pem");

            mainTabControl.Items.Add(new TabItem() { Header = new DisasmTabHeader(viewer, mainTabControl), Content = viewer });
            mainTabControl.SelectedIndex = mainTabControl.Items.Count - 1;
        }


        private void compileButton_Click(object sender, RoutedEventArgs e)
        {
            var tabItem = mainTabControl.SelectedItem as TabItem;
            if (tabItem.Header is CustomTabHeader)
            {
                var header = tabItem.Header as CustomTabHeader;
                //header.SaveFile();
                try
                {
                    header.Compile();
                }catch (Exception e_)
                {
                    this.MessageTextBox.AppendText(e_.Message);
                }
                
                /*file = header.Path + header.FileName;


                ProcessStartInfo compilerProcess = new ProcessStartInfo();
                compilerProcess.UseShellExecute = false;
                compilerProcess.WorkingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Compiler";
                compilerProcess.FileName = "Compiler/PeakC.exe";
                compilerProcess.Arguments = file + " -o " + "../Output/";
                compilerProcess.RedirectStandardOutput = true;
                compilerProcess.RedirectStandardError = true;

                if (Directory.Exists("Output") == false)
                    Directory.CreateDirectory("Output");

                //compilerProcess.
                //Process.Start(compilerProcess);
                var proc = new Process();
                proc.StartInfo = compilerProcess;
                
                proc.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
                proc.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);
                try
                {
                    proc.Start();
                }catch (Exception e_)
                {
                    MessageBox.Show(e_.Message);
                    return;
                }
                
                
                string output = proc.StandardOutput.ReadToEnd();
                //proc.WaitForExit();

                MessageTextBox.AppendText(output);*/
            }



        }

        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            MessageTextBox.AppendText(outLine.Data);
        }

        private void run(object sender, RoutedEventArgs e)
        {
            var tabItem = mainTabControl.SelectedItem as TabItem;
            if (tabItem.Header is CustomTabHeader)
            {
                var header = tabItem.Header as CustomTabHeader;

                try
                {
                    header.Run();
                }
                catch(Exception e_)
                {
                    this.MessageTextBox.AppendText(e_.Message);
                }
                
            }
        }

        public class CustomTabHeader : System.Windows.Controls.UserControl
        {
            private int index;
            private TabControl tabControl;
            private TextEditor editor;
            private MainWindow ideWindow;
            public string FileName;
            public string Path;
            private bool notSaved = true;
            public CustomTabHeader(int index, TabControl tabControl, TextEditor editor, MainWindow mainWindow, string fileName, string path)
            {
                this.index = index;
                this.tabControl = tabControl;
                this.editor = editor;
                this.ideWindow = mainWindow;
                this.FileName = fileName;
                this.Path = path;

                var tabContent = new StackPanel() { Orientation = Orientation.Horizontal };
                tabContent.Children.Add(new Label() { Content = fileName });
                tabContent.Children.Add(new Button()
                {
                    Content = "[X]",
                    Height = 20,
                    Width = 20,
                    Background = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 0, 0) },
                    BorderBrush = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 0, 0) },
                });
                (tabContent.Children[tabContent.Children.Count - 1] as Button).Click += CloseTabClicked;
                editor.KeyDown += Editor_KeyDown;
                this.Content = tabContent;
                if (path != "")
                {
                    editor.Text = File.ReadAllText(path + fileName);
                    notSaved = false;
                }
                else
                {
                    this.notSaved = true;
                    CheckNotSavedFlag();
                }
            }

            private void Editor_KeyDown(object sender, KeyEventArgs e)
            {
                if (notSaved == false)
                {
                    notSaved = true;
                    CheckNotSavedFlag();
                }
            }

            public void CloseTabClicked(object sender, RoutedEventArgs e)
            {
                this.tabControl.Items.RemoveAt(index);
            }

            public void SaveFile()
            {
                var dialog = new SaveFileDialog() { Filter = "Peak project (*.p)|*.p" };
                if (Path.Length == 0)
                    if (dialog.ShowDialog() == true)
                    {
                        File.WriteAllText(dialog.FileName, editor.Text);
                        //this.Path = dialog.FileName.Substring(dialog.FileName.Length - Path.Length - 1);
                        this.FileName = getFileName(dialog.FileName);
                        this.Path = getFilePath(dialog.FileName);

                    }
                    else
                        return;
                else
                    File.WriteAllText(Path + FileName, editor.Text);

                notSaved = false;
                CheckNotSavedFlag();
            }

            public void Compile()
            {
                this.SaveFile();
                var file = this.Path + this.FileName;


                ProcessStartInfo compilerProcess = new ProcessStartInfo();
                compilerProcess.UseShellExecute = false;
                compilerProcess.WorkingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Compiler";
                compilerProcess.FileName = "Compiler/PeakC.exe";
                compilerProcess.Arguments = file + " -o " + "../Output/";
                compilerProcess.RedirectStandardOutput = true;
                compilerProcess.RedirectStandardError = true;

                if (Directory.Exists("Output") == false)
                    Directory.CreateDirectory("Output");

                //compilerProcess.
                //Process.Start(compilerProcess);
                var proc = new Process();
                proc.StartInfo = compilerProcess;

                proc.OutputDataReceived += new DataReceivedEventHandler(ideWindow.OutputHandler);
                proc.ErrorDataReceived += new DataReceivedEventHandler(ideWindow.OutputHandler);
                try
                {
                    proc.Start();
                }
                catch (Exception e_)
                {
                    MessageBox.Show(e_.Message);
                    return;
                }


                string output = proc.StandardOutput.ReadToEnd();
                //proc.WaitForExit();

                ideWindow.MessageTextBox.AppendText(output);

                MakeAssembly();
                ideWindow.MessageTextBox.AppendText(".exe file placed in "+this.Path+"Output");
            }

            public void MakeAssembly()
            {
                if (Path[Path.Length - 1] != '/')
                    Path.Append('/');

                if (Directory.Exists(Path + "Output") == false)
                    Directory.CreateDirectory(Path + "Output");

                clearFolder(Path + "Output");

                var fileName = removeFileExtention(FileName);

                FileSystem.CopyDirectory("Output", Path + "Output", true);
                FileSystem.CopyDirectory("Runtime", Path + "Output", true);

                try
                {
                    FileSystem.RenameFile(Path + "Output/" + "RuntimeEnvironment.exe", fileName + ".exe");
                }
                catch (Exception e_) { }
                
            }
            private string getFileName(string fileName)
            {
                string S = "";

                for (int i = fileName.Length - 1; i > 0; i--)
                {
                    if (fileName[i] != '\\' && fileName[i] != '/')
                        S = fileName[i] + S;
                    else return S;
                }
                return S;
            }

            private string removeFileExtention(string fileName)
            {
                for (int i = fileName.Length - 1; i>0; i--)
                {
                    if (fileName[i] == '.')
                    {
                        return fileName.Substring(0, i);
                    }
                }
                return fileName;
                //throw new Exception();
            }

            private void clearFolder(string path)
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                foreach (FileInfo fi in dir.GetFiles())
                    fi.Delete();
                
                /*
                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    clearFolder(di.FullName);
                    di.Delete();
                }*/
            }
            private string getFilePath(string fileName)
            {
                string S = "";

                for (int i = fileName.Length - 1; i > 0; i--)
                {
                    if (fileName[i] == '\\' || fileName[i] == '/')
                        return fileName.Substring(0, i + 1);
                }
                return S;
            }
            public void CheckNotSavedFlag()
            {
                if (notSaved)
                {
                    var label = (this.Content as StackPanel).Children[0] as Label;
                    if ((label.Content as string)[(label.Content as string).Length - 1] != '*')
                    {
                        label.Content += "*";
                    }
                }
                else
                {
                    var label = (this.Content as StackPanel).Children[0] as Label;
                    if ((label.Content as string)[(label.Content as string).Length - 1] == '*')
                    {
                        string S = label.Content.ToString();
                        label.Content = S.Remove(S.Length - 1);
                    }
                }
            }

            public void Run()
            {
                //Process.Start(Path + "Output/" + removeFileExtention(FileName) + ".exe");

                Process proc = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = Path + "Output";
                info.FileName = Path + "Output/" + removeFileExtention(FileName) + ".exe";
                proc.StartInfo = info;
                proc.Start();
            }
        }
    }

    public class DisasmTabHeader : System.Windows.Controls.UserControl
    {
        private DisAsmViewer viewer;
        private TabControl tabControl;
        private int index;
        public DisasmTabHeader(DisAsmViewer viewer, TabControl tabControl)
        {
            this.viewer = viewer;
            this.tabControl = tabControl;

            var tabContent = new StackPanel() { Orientation = Orientation.Horizontal };
            tabContent.Children.Add(new Label() { Content = "DISASM" });
            tabContent.Children.Add(new Button()
            {
                Content = "[X]",
                Height = 20,
                Width = 20,
                Background = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 0, 0) },
                BorderBrush = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 0, 0) },
            });
            (tabContent.Children[tabContent.Children.Count - 1] as Button).Click += CloseTabClicked;
            this.Content = tabContent;
            this.index = tabControl.Items.Count - 1;
        }

        private void CloseTabClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tabControl.Items.RemoveAt(index);
            }
            catch (ArgumentOutOfRangeException e1) { return; }
        }
    }
}
