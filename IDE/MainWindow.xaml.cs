using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            makeNewTab("new.p");
            insertBaseText();
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

            var tabHeader = new CustomTabHeader(mainTabControl.Items.Count, mainTabControl, editor, fileName, path);


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

            mainTabControl.Items.Add(new TabItem(){ Header = new DisasmTabHeader(viewer, mainTabControl), Content = viewer});
            mainTabControl.SelectedIndex = mainTabControl.Items.Count - 1;
        }
    }

    public class CustomTabHeader : System.Windows.Controls.UserControl
    {
        private int index;
        private TabControl tabControl;
        private TextEditor editor;
        private string fileName;
        private string path;
        private bool notSaved = true;
        public CustomTabHeader(int index, TabControl tabControl, TextEditor editor, string fileName, string path)
        {
            this.index = index;
            this.tabControl = tabControl;
            this.editor = editor;
            this.fileName = fileName;
            this.path = path;

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
            if (path!="")
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
            if (path.Length == 0)
                if (dialog.ShowDialog() == true)
                {
                    File.WriteAllText(dialog.FileName, editor.Text);
                    this.path = dialog.FileName.Substring(dialog.FileName.Length - path.Length - 1);
                }
                else
                    return;
            else
                File.WriteAllText(path + fileName, editor.Text);

            notSaved = false;
            CheckNotSavedFlag();
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
            this.index = tabControl.Items.Count;
        }

        private void CloseTabClicked(object sender, RoutedEventArgs e)
        {
            this.tabControl.Items.RemoveAt(index);
        }
    }
}
