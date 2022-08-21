using System;
using System.Collections.Generic;
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

namespace PDE
{
    /// <summary>
    /// Interaction logic for TabControl.xaml
    /// </summary>
    public partial class TabControl : UserControl
    {
        public MainWindow PDE_Window { get; internal set; }

        public TabControl()
        {
            InitializeComponent();            
        }
        //public TabHeader SelectedTab { get; set; }
        public TabHeader AddTabWithSimpleText()
        {
            var header = new TabHeader(this); 
            header.CreateWithText("load \"std.p\"; \nprint_i(1);");
            //SelectedTab = header;
            return header;
        }
        public void AddTab(TabHeader header)
        {
            this.tabStack.Children.Add(header);
            this.TextGrid.Children.Clear();
            this.TextGrid.Children.Add(header.Editor);
        }
        public void Click() 
        {
        
        }
        public TabHeader GetSelectedTab()
        {
            foreach (var e in this.tabStack.Children)
                if ((e as TabHeader).IsSelect)
                    return e as TabHeader;
            return null;
        }
        public void SelectLast()
        {
            if (this.tabStack.Children.Count > 0)
                (this.tabStack.Children[this.tabStack.Children.Count - 1] as TabHeader).Select();
        }
    }
}
