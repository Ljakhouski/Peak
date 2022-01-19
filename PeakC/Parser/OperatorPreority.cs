using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Parser
{
    /*
     Program ->  <func> | <proc> | <if> | <while> | <new_var_expr> | <proc_call> | <dot_expr> | <...>
     <func> -> 'func' + '(' + <type_expr> + ')' + <name> + '(' + <>
     
     
     */
    class OperatorPreority
    {
        public static string[][] Preority =
        {

                    new string[]{  ","                                  },
                    new string[]{  "++", "--",                          },
                    new string[]{  ":",                                 },
                    new string[]{  "and", "or" ,                        },
                    new string[]{  "=", "!=" , ">", "<", ">=", "<=",    },
                    new string[]{  "+", "-",                            },
                    new string[]{  "*", "/",                            },
                    new string[]{  "[]", "."                            }

        };
    }
}
