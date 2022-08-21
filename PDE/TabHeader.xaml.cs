using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace PDE
{
    /// <summary>
    /// Interaction logic for TabHeader.xaml
    /// </summary>
    public partial class TabHeader : UserControl
    {
        public TabHeader(TabControl container)
        {
            InitializeComponent();
            this.Container = container;
            Editor = new TextEditor();
            Editor.KeyDown += Editor_KeyDown;
            Editor.ShowLineNumbers = true;
            Editor.FontFamily = new FontFamily("Consolas");
            Editor.FontSize = 16;
            Editor.Background = new SolidColorBrush(Color.FromArgb(0,0,0,0));
            loadHighlighter(Editor);
            Container.tabStack.Children.Add(this);
            Select();
        }

        private void Editor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
                this.Save();
            else if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control)
                this.Container.PDE_Window.Open_Button_Click(null, null);
            else if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
                this.Container.PDE_Window.New_Button_Click(null, null);
            else if (e.Key == Key.F5)
                Compile();
            else
            {
                this.IsSaved = false;
                UpdateTabLabel();
            }
        }

        internal void Run()
        {
            try
            {
                var exePath = FileNames.GetExeName(FilePath);

                Process proc = new Process();
                ProcessStartInfo info = new ProcessStartInfo();
                info.WorkingDirectory = FileNames.GetFilePath(exePath);
                info.FileName = exePath;
                proc.StartInfo = info;
                proc.Start();
            }
            catch(Exception e)
            {
                Container.PDE_Window.ConsoleTextBox.AppendText($"\n{e.Message}\n");
            }
        }

        public void Compile()
        {
            this.Save();

            ProcessStartInfo compilerProcess = new ProcessStartInfo();
            compilerProcess.CreateNoWindow = true;
            compilerProcess.WindowStyle = ProcessWindowStyle.Hidden;
            compilerProcess.UseShellExecute = false;
            compilerProcess.WorkingDirectory = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\Compiler";
            compilerProcess.FileName = "Compiler/PeakC.exe";
            compilerProcess.Arguments = FilePath;
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
            }
            catch (Exception e_)
            {
                MessageBox.Show(e_.Message);
                IsCompiled = false;
                return;
            }


            string output = proc.StandardOutput.ReadToEnd();
            //proc.WaitForExit();

            Container.PDE_Window.ConsoleTextBox.AppendText(output);

            if (IsCompiled)
            {
                Container.PDE_Window.ConsoleTextBox.AppendText($"\n.exe file placed in {FilePath}");
            }
        }
        public void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            //Compiled = true;
            Container.PDE_Window.ConsoleTextBox.AppendText(outLine.Data);
            if (outLine.Data.Contains("Error") == false &&
                outLine.Data.Contains("error") == false)
            {
                IsCompiled = true;
            }
        }

        public void UpdateTabLabel()
        {
            this.tabLabel.Content = this.TabLabel;
        }
        public TextEditor Editor { get; set; } 
        public TabControl Container { get; set; }
        public string FilePath { get; set; } = "";
        public string TabLabel { get {
                char ch = '*';
                if (IsSaved) 
                    ch = ' ';

                if (this.FilePath == "")
                    return "unknow.p" + ch;
                else
                    return FileNames.GetFileName(FilePath)+ch;
            } }
        public bool IsSaved { get; set; }
        public bool IsCompiled { get; set; } = false;

        public bool IsSelect { get{
                return this.selectedIndicator.Visibility == Visibility.Visible;
            } }

        public DataReceivedEventHandler PrintLineToConsole { get; private set; }

        public void Select()
        {
            foreach (var e in Container.tabStack.Children)
                if (e is TabHeader)
                    (e as TabHeader).Unselect();

            this.selectedIndicator.Visibility = Visibility.Visible;
            this.Container.TextGrid.Children.Clear();
            this.Container.TextGrid.Children.Add(this.Editor);
            //this.Container.SelectedTab = this;
        }

        public void Unselect()
        {
            this.selectedIndicator.Visibility = Visibility.Hidden;
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
            editor.Foreground = new SolidColorBrush(Color.FromRgb(222,222,222));
            editor.SyntaxHighlighting = HighlightingLoader.Load(xshd, HighlightingManager.Instance);
        }


        private void onClick(object sender, MouseButtonEventArgs e)
        {
            var stack = Container.tabStack.Children;
            foreach (TabHeader header in stack)
            {
                header.Unselect();
            }
            this.Select();
        }

        public void OpenFromFile()
        {
            var dialog = new System.Windows.Forms.OpenFileDialog() { Filter = "Peak project (*.p)|*.p|All files (*.*)|*.*" };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Editor.Text = File.ReadAllText(dialog.FileName);
                //this.TabLabel; = FileNames.GetFileName(dialog.FileName);
                this.IsSaved = true;
                this.FilePath = dialog.FileName;
                UpdateTabLabel();
            }

            //this.Container.tabStack.Children.Clear();
            //this.Container.tabStack.Children.Add(this.Editor);
        }
        public void CreateWithText(string text)
        {
            this.Editor.Text = text;
            //this.TabLabel;// = "unknow.p";
            this.IsSaved = false;
            UpdateTabLabel();
        }

        private void CloseTab(object sender, RoutedEventArgs e)
        {
            if (this.IsSaved == false)
                this.Save();
            this.Container.tabStack.Children.Remove(this);
            this.Container.TextGrid.Children.Clear();
            this.Container.SelectLast();
        }

        public void Save()
        {
            var dialog = new System.Windows.Forms.SaveFileDialog() { Filter = "Peak project (*.p)|*.p" };
            if (this.FilePath.Length == 0)
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.WriteAllText(dialog.FileName, this.Editor.Text);
                    //this.Path = dialog.FileName.Substring(dialog.FileName.Length - Path.Length - 1);
                    this.FilePath = dialog.FileName;
                }
                else
                    return;
            else
                File.WriteAllText(this.FilePath, this.Editor.Text);

            IsSaved = true;
            UpdateTabLabel();
        }
    }
}
