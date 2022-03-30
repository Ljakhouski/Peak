#load "std";
func (int) div (int a, int b) operator /
[
	native func (int) int_division (int a, int b);
	if b = 0
	[
		print("division error!");
		return 0;
	]
	else
	[
		return int_division(a,b);
	]
]