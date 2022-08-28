import proc nextLine() from "stdlib.dll";

proc print(int i)
[
    import proc print_i(int i) from "stdlib.dll";
    print_i(i);
]

proc printn(int i)
[
    import proc print_i(int i) from "stdlib.dll";
    print_i(i);
    nextLine();
]


/*
proc print(double d)
[
    import proc print_d(double d) from "stdlib.dll";
    print_d(d);
]*/

import func (int) getInt() from "stdlib.dll";

import func (int) getHours        () from "stdlib.dll";
import func (int) getMinutes      () from "stdlib.dll";
import func (int) getSeconds      () from "stdlib.dll";
import func (int) getMilliseconds () from "stdlib.dll";
