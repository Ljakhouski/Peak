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


namespace PDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.mainTabControl.PDE_Window = this;
            this.mainTabControl.AddTabWithSimpleText();
            this.ConsoleTextBox.AppendText($"IDE started {DateTime.Now}");
        }

        public void New_Button_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.AddTabWithSimpleText();
        }

        public void Open_Button_Click(object sender, RoutedEventArgs e)
        {
            var header = new TabHeader(this.mainTabControl);
            header.OpenFromFile();
            //this.mainTabControl.AddTab(header);
        }

        public void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            mainTabControl.GetSelectedTab().Save();
        }

        public void CompileButton_Click(object sender, RoutedEventArgs e)
        {

        }

        public void Run(object sender, RoutedEventArgs e)
        {

        }

        public void ViewDisasm(object sender, RoutedEventArgs e)
        {

        }

       
    }
}
