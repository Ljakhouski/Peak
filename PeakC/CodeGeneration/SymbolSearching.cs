
using Peak.PeakC;
using RuntimeEnvironment.RuntimeModule;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Text;

namespace Peak.CodeGeneration
{
    partial class SymbolTable
    {
        public bool ContainsHere(Token name) // only for visitor (access-code-generator)
        {
            foreach (TableElement t in Data)
            {
                if (t.Name == name.Content)
                    return true;
            }
            return false;
        }

        public bool ContainsInAllTables(Token name)
        {
            foreach (TableElement t in Data)
            {
                if (t.Name == name.Content)
                    return true;
                else if (t.Type.Value == SymbolType.Type.RefOnMethodContext)
                {
                    if (t.MethodContextTable.ContainsInAllTables(name))
                        return true;
                }
            }
            if (Prev != null)
                return Prev.ContainsInAllTables(name);
            else
            return false;
        }

        public TableElement GetSymbol(Token name)
        {
            for (int i = Data.Count - 1; i >= 0; i--)
            {
                var t = Data[i];

                if (t.Name == name.Content)
                    return t;
                else if (t.Type.Value == SymbolType.Type.RefOnMethodContext)
                {
                    return t.MethodContextTable.GetSymbol(name);
                }
            }

            if (IsGlobalScopeTable)
                return null;
            else
                return Prev.GetSymbol(name);
        }

        public TableElement[] GetSymbols(Token name) // [] - for proc & func
        {
            var symbols = new List<TableElement>();

            for (int i = Data.Count - 1; i>=0; i--)
            {
                var t = Data[i];

                if (t.Name == name.Content)
                    symbols.Add(t);
                else if (t.Type.Value == SymbolType.Type.RefOnMethodContext)
                {
                    symbols.AddRange(t.MethodContextTable.GetSymbols(name));
                }
            }

            if (IsGlobalScopeTable)
                return symbols.ToArray();
            else
            {
                symbols.AddRange(Prev.GetSymbols(name));
                return symbols.ToArray();
            }
        }

    }
}
