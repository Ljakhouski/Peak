load "std.p";

//proc Foo(int i)[ printn(6);Foo(3);]
//Foo(3);
while 1=1
[
	int i <- getInt();
	
	while i>0
	[
		printn(i);
		i <- i-1;
	]
	
]
