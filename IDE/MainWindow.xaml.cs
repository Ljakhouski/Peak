using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

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

        private void makeNewTab(string fileName)
        {
            var editor = new TextEditor();
            editor.ShowLineNumbers = true;
            editor.FontFamily = new FontFamily("Consolas");
            editor.FontSize = 16;
            //editor.Background = new SolidColorBrush() { Color = Color.FromArgb(10, 10, 20, 20) };
            loadHighlighter(editor);

            var tabContent = new StackPanel() { Orientation = Orientation.Horizontal };
            tabContent.Children.Add(new Label() { Content = fileName });
            tabContent.Children.Add(new Button() { 
                Content = "[X]",
                Height = 20,
                Width = 20,
                Background = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 0, 0) },
                BorderBrush = new SolidColorBrush() { Color = Color.FromArgb(0, 0, 0, 0) },
            });
            
        
            mainTabControl.Items.Add(new TabItem() { Content = editor, Header = /*"new.p"*/  tabContent});
        }

        private void insertBaseText()
        {
            (((TabItem)mainTabControl.Items[mainTabControl.Items.Count - 1]).Content as TextEditor).Text
            = "load \"std.p\";\nprint(\"Hello World\");";
        }
    }
}
