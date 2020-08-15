using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Parser
{
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
