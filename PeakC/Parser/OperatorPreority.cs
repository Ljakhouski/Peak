using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Parser
{
    /*
     Program ->  <func> | <proc> | <if> | <while> | <new_var_expr> | <proc_call> | <dot_expr> | <...>
     <func> -> 'func' + '(' + <type_expr> + ')' + <name> + '(' + <>
     
     
     */

    static class NonterminalPreority
    {
        // old
        /*public static string[][] Preority =
        {

                    new string[]{  ","                                  },
                    new string[]{  "++", "--",                          },
                    new string[]{  ":",                                 },
                    new string[]{  "and", "or" ,                        },
                    new string[]{  "=", "!=" , ">", "<", ">=", "<=",    },
                    new string[]{  "+", "-",                            },
                    new string[]{  "*", "/",                            },
                    new string[]{  "[]", "."                            }
        };*/

        public static List<Nonterminal> Preority;

        public static void MakePriorityList()
        {
            Preority = new List<Nonterminal>();

            Preority.Add(new Nonterminal() { Type = NonterminalType.Program, IsBinary = false });
            Preority.Add(new Nonterminal() { Type = NonterminalType.CodeBlock, IsBinary = false});
            Preority.Add(new Nonterminal() { Type = NonterminalType.Args, IsBinary = false });
            Preority.Add(new Nonterminal() { Type = NonterminalType.Modifier, IsBinary = false });
            Preority.Add(new Nonterminal() { Type = NonterminalType.Sequence, IsBinary = false, Terminals = new List<string> { "," } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.AndOr, IsBinary = true, Terminals = new List<string> { "and", "or" } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.Assignment, IsBinary = true, Terminals = new List<string> { "<<" } });
           // Preority.Add(new Nonterminal() { Type = NonterminalType.IncrementOrDecrement, IsBinary = false, Terminals = new List<string> { "++", "--" } });
           // Preority.Add(new Nonterminal() { Type = NonterminalType.DoubleDot, IsBinary = false, Terminals = new List<string> { ":" } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.Equals, IsBinary = true, Terminals = new List<string> { "==" } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.Comparison, IsBinary = true, Terminals = new List<string> { "=", "!=", ">", "<", ">=", "<=" } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.PlusMinus, IsBinary = true, Terminals = new List<string> { "+", "-" } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.MulDiv, IsBinary = true, Terminals = new List<string> { "*", "/" } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.Dot, IsBinary = false, Terminals = new List<string> { "." } });
            Preority.Add(new Nonterminal() { Type = NonterminalType.MethodCall, IsBinary = false});
            Preority.Add(new Nonterminal() { Type = NonterminalType.Data, IsBinary = false });
            //Preority.Add(new Nonterminal() { Type = NonterminalType.Double, IsBinary = false, Terminals = new List<string> { "." } });
            //Preority.Add(new Nonterminal() { Type = NonterminalType.Name, IsBinary = false, Terminals = new List<string> { "," } });
            
        }
        public static Nonterminal GetNextByPreority(Nonterminal n) // or "take"...
        {
            try
            {
                for (int i = 0; i < Preority.Count; i++)
                    if (Preority[i] == n)
                        return Preority[i + 1];
                throw new NonterminalPreorityException();
            }
            catch(IndexOutOfRangeException e)
            {
                throw new NonterminalPreorityException();
            }
        }

        public static Nonterminal GetNextByPreority(NonterminalType type)
        {
            return GetNextByPreority(GetByType(type));
        }
        public static Nonterminal GetByType(NonterminalType type)
        {
            foreach (Nonterminal n in NonterminalPreority.Preority)
                if (n.Type == type)
                    return n;
            throw new Exception();
        }
    }
    enum NonterminalType
    {
        Program, // contains <Entry>, global scope
        CodeBlock, // if-else | while | load | func/proc declaration | expression
        Type,
        Modifier,
        Sequence, // <expr> {',' <expr>} | <expr>
        Args,     // <type_expr> { ',' <type_expr> } | <type_expr>
        Assignment,
        IncrementOrDecrement,
        DoubleDot,
        AndOr,
        Equals,
        Comparison,
        PlusMinus,
        MulDiv,
        Dot,    // contains array-access-expression // ???
        Data,   // Names (ID) | func calling | array-access-expression
        Double, // <int_const>, ".", <int_const>
        MethodCall, // <ID>, '(', <args expression>, ')'
    }
    class Nonterminal
    {
        public NonterminalType Type { get; set; }
        public List<string> Terminals { get; set; }
        public bool IsBinary { get; set; }

    }

    class NonterminalPreorityException : Exception
    {

    }
}
