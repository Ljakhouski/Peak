using RuntimeEnvironment.RuntimeModule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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

namespace IDE
{
    /// <summary>
    /// Логика взаимодействия для DisAsmViewer.xaml
    /// </summary>
    public partial class DisAsmViewer : UserControl
    {
        public DisAsmViewer()
        {
            InitializeComponent();
        }

        public void MakeDisasbInfo(string path)
        {
            RuntimeModule diasmSource;
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                diasmSource = (RuntimeModule)formatter.Deserialize(fs);
            }

            //this.textBox.Text
            foreach (string S in getDisasmLines(diasmSource))
                this.textBox.AppendText(S+ "\n");
        }

        private List<string> getDisasmLines(RuntimeModule module)
        {
            var asm = new List<string>();
            asm.Add(".info\n");
            asm.Add("   module name:     "   + module.ModuleName);
            asm.Add("   constant size:   "   + module.Constant.Length);
            asm.Add("   methods:         "   + module.Methods.Length);
            asm.Add(".const\n");
            foreach (Constant c in module.Constant)
            {
                string value;
                switch (c.Type)
                {
                    case ConstantType.Bool:
                        value = c.BoolValue.ToString();
                        break;
                    case ConstantType.Int:
                        value = c.IntValue.ToString();
                        break;
                    case ConstantType.Double:
                        value = c.DoubleValue.ToString();
                        break;
                    case ConstantType.Str:
                        value = c.StrValue.ToString();
                        break;
                    default:
                        value = "UNKNOW";
                        break;
                }
                asm.Add("   type: " + c.Type.ToString() + "   value: "+value); ;
            }

            foreach (MethodDescription method in module.Methods)
            {
                asm.Add(".method \""+ method.Name+"\"  .size: "+method.LocalVarsArraySize);
                asm.Add("\n.code");

                int i = 0;
                foreach (Instruction opCode in method.Code)
                {
                    string operands="";

                    if (opCode.Operands != null)
                        foreach (int op in opCode.Operands)
                            operands += op + " ";

                    string spaces = "   ";
                    for (int j = 0; j < i.ToString().Length; j++)
                        if (j<5)
                            spaces = spaces.Remove(0, 1);

                    asm.Add("   " + i.ToString() + spaces +"  " + opCode.Name.ToString()+"   "+operands);
                    i++;
                }
                asm.Add("");
                asm.Add("__________________________");
                asm.Add("");
            }

            return asm;
        }
    }
}
