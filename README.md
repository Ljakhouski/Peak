# Peak
My new test programming language


    ***********
    * GRAMMAR *
    ***********

// - comment
/*  comment  */

load "std"              // loading libraries
define CLEAR c : 0      // preprocessor definition

? S: "random string"   // create a new variable & set value (auto type)
? i: 12345  

str data: "09.08.20"   // create a new variable with static type
int year: 2020

#Pi : 3.14159          // create constant


                       //  function declaration:

i2 : i1                // set variable

if a=b 
[
    /*...*/
]

while a=b
[
    /*...*/
]

                        // function and procedure declaration:
func (int) add (int a, int b)
[
    << a+b      // return value
]

proc increment
[
    i++;
]

                        // native function declaration:
                        // (& overloading)

native proc print (int integer) print_i
native proc print (str string) print_s
native proc next_line

native func (int) read read_i
native func (str) read read_s
print ("Hello world!")

                        // structures
struct Person
[
    ?age : 0
    ?name : ""

    proc run
    [
        /*....*/
    ]
]

?Mike : new Person
?Mike.age = 24
?Mike.run()


/****CALC*****/

?a int
?b int

? op str

while true
[
    print("operation? (+-*/): "); op : read()   // operator ";"
    print("a: ")
    a : read()
    print("b: ")
    b : read()

    if op = "+"
    [
        print( a+b )
    ]
    else if op = "-"
    [
        print ( a-b )
    ]
    else if op = "*"
    [
        print ( a*b )
    ]
    else if op = "/"
    [
        if b = 0 [print ("error! division on null")]
        else
        [
            print (a/b)
        ] 
    ]
]


