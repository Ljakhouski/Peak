load "std";

native proc print(str s);
native proc print (str s);

print("Hello World");

native proc summ(int a, int b, int c);
//summ(1,2,3);

proc summ(int a, int b)
[
	return (a+b);
]

struct A
[
	str _name 
	array[str] << this._name;
	int _i;
	proc get_I()
	[
		return i;
	]
	
	A
]

func () foo()[ func () bar()[ /* some code...*/]]
struct Foo[ func () foo()[ func () bar() [  ] ]]

if a=b 
[
	print ("equals");
]
else
[
	print ("different");
]

#i<<0; while i>255
[
	i<<i+1;
]
for #t << 1 to 255
[
	// some code		
]