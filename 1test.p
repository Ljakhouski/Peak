int a << 3;
#a1 << 345345;
int b << a;
int c << b;
int d << c;
int e << d;
int summ << a+b+c+d;
double d2 << 0.0;
native proc print (int i);
native proc print (double d2);
print(summ);
print(d2);