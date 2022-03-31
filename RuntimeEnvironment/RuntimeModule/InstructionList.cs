using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    public enum InstructionName
    {
        Push,           // <address>  - push on value-stack from frame-stack
        PushGlobal,     // <address>  - push on value-stack from 0-element from frame-stack
        PushByRef,      //            - push on value-stack by address (int value on value-stack) in object on value-stack-top
        
        Store,            // <address>  - pop in frame-stack from value-stack
        StoreGlobal,      // <address>  - pop in frame-stack from 0-element from value-stack
        StoreByRef,       //            - pop by address (int value on value-stack) in object from value-stack-top 
        
        PushConst, // <address>

        Add,
        Sub,
        Mul,
        Div,

      /*Add_i,
        Sub_i,
        Mul_i,
        Div_i,*/

      /*Add_d,
        Sub_d,
        Mul_d,
        Div_d,*/

        Jump,
        Return,
        CallNative,
        Call,
        Pop,
        IfNot,

        Equals,
        More,
        Less,
    }
}
