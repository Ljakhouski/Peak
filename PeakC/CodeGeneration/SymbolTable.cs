using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace Peak.CodeGeneration
{
    class SymbolTable
    {
        private List<string> loadetFiles = new List<string>();
        public bool IsGlobalScopeTable { get; set; } = false;
        public SymbolTable Prev { get; set; }
        public SymbolTable Next { get; set; }

        public List<TableElement> Data = new List<TableElement>();
        public List<RuntimeEnvironment.RuntimeModule.Constant> ConstandData = new List<RuntimeEnvironment.RuntimeModule.Constant>();
        public int MemorySize { get; set; }

        public int GeneratedMethodAddress { get; set; }
        public SymbolTable()
        {

        }

        public bool IsNewFile(string file)
        {

            if (this.loadetFiles.Contains(Path.GetFullPath(file)))
                return true;
            return false;
        }

        public void RegisterFile(string file)
        {
            this.loadetFiles.Add(file);
        }

        private int calculateNewOffsetAddress() // for last element
        {
            var table = this;

            while (true)
            {
                for (int i = Data.Count - 1; i >= 0; i--)
                {
                    if (Data[i].OffsetAddress != -1)
                    {
                        expandMemorySizeByLastAddress(Data[i].OffsetAddress + 1);
                        return Data[i].OffsetAddress + 1;
                    }

                }

                if (table.IsGlobalScopeTable)
                {
                    expandMemorySizeByLastAddress(0);
                    return 0;
                }

                /*else if (table.IsMethodScope)
                {

                }*/
                else
                    table = table.Prev;
            }
        }

        public bool ContainsSymbol(Token name)
        {
            foreach (TableElement t in Data)
            {
                if (t.Name == name.Content)
                    return true;
            }
            if (Prev != null)
                return Prev.ContainsSymbol(name);
            else
                return false;
        }

        public void RegisterSymbol(TableElement tableElement)
        {
            tableElement.Ref = this;
            tableElement.OffsetAddress = calculateNewOffsetAddress();
            this.Data.Add(tableElement);
            //if ()
        }


        private void expandMemorySizeByLastAddress(int size)
        {
            if (this.MemorySize < size + 1)
                MemorySize = size + 1;
        }
        public int GetConstantAddress(ConstValueNode node)
        {
            if (node.Value.Type == type.IntValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Int,
                    IntValue = int.Parse(node.Value.Content)
                });
                return ConstandData.Count - 1;
            }
            else if (node.Value.Type == type.DoubleValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Double,
                    DoubleValue = double.Parse(node.Value.Content, CultureInfo.InvariantCulture)
                });
                return ConstandData.Count - 1;
            }
            else if (node.Value.Type == type.BoolValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Bool,
                    BoolValue = bool.Parse(node.Value.Content)
                });
                return ConstandData.Count - 1;
            }
            else if (node.Value.Type == type.StrValue)
            {
                this.ConstandData.Add(new RuntimeEnvironment.RuntimeModule.Constant()
                {
                    Type = RuntimeEnvironment.RuntimeModule.ConstantType.Str,
                    StrValue = node.Value.Content
                });
                return ConstandData.Count - 1;
            }
            else
                throw new Exception();
        }
    }
}
