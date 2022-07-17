# Peak

Peak - is the my new test programming language

It is the x86_64 compiler now, but long time he was the interpreter (*.p files were translating into byte-code for my own runtime)
compiler translates peak-code to the FASM code and makes .exe or .dll file
only for x86_64 architecture (intel & AMD)

build project and run IDE.exe to open sample-projects and try 


##  SYNTAX  


    // - comment
    /*  comment  */

    load "std"              // loading libraries
    define Pi 3.14159       // preprocessor definition

    #S <- "random string"      // create a new variable & set value (auto type)
    str S <- "random string";  // explicit type specification

    S <- "new random string";
    Pi <- 3.1;                  // fuu how rude

    $Pi <- 3.14159;          // create constant
    int Pi <- 3.14159;


    func (int) summ (int a, int b); //  function declaration:
    proc add(int i);

    import func (str) scan from "stdrl.dll";
    import func (int) scanInt from "stdrl.dll";


    if a = b 
    [
        /*...*/
    ]

    while a = b
    [
        /*...*/
    ]

    // function and procedure declaration:
    func (int) add (int a, int b)
    [
        return a+b      // return value
    ]

    proc increment
    [
        i <- i + 1;
    ]


## console calc

    int a;
    int b;
    str op;

    while true
    [
        print("operation? (+-*/): ");
        op <- read();
        print("a: ");
        a <- readInt();
        print("b: ");
        b <- readInt();

        if op = "+"
        [
            print( a+b );
        ]
        else if op = "-"
        [
            print ( a-b );
        ]
        else if op = "*"
        [
            print ( a*b );
        ]
        else if op = "/"
        [
            if b = 0
            [
                print ("error! division on null");
            ]
            else
            [
                print (a/b);
            ] 
        ]
    ]


