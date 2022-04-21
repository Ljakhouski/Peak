proc print(str s)
[
    native proc print_s(str s);
    print_s(s);
]

proc print(int i)
[
    native proc print_i(int i);
    print_i(i);
]

proc print(double d)
[
    native proc print_d(double d);
    print_d(d);
]

//native func (str) read();