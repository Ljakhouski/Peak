/****CONSOLE IN OUT****/

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

native func (str)    read();
native func (int)    readInt();
native func (double) readD();

/****TYPES CONVERTATION****/

func (int) toInt(str s)
[
    native func (int) str_to_int(str s);
    return str_to_int(s);
]

func (int) toInt(double d)
[
    native func (int) double_to_int(double d);
    return double_to_int(d);
]

func (double) toDouble(str s)
[
    native func (double) str_to_double(str s);
    return str_to_double(s);
]

func (double) toDouble(int i)
[
    native func (double) int_to_double(int i);
    return int_to_double(i);
]