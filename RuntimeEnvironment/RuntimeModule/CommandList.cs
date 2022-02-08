using System;
using System.Collections.Generic;
using System.Text;

namespace RuntimeEnvironment.RuntimeModule
{
    public enum CommandName
    {
        Push, // <address>        - push on value-stack from frame-stack
        PushStatic, // <address>  - push on value-stack from 0-element from frame-stack
        PushByRef, //  <address>  - push on value-stack by address in object on value-stack-top
        Pop,
        PopStatic,
        PopByRef,
        Return,
        PushConst,

        Set, // a = b

        Add,
        Sub,
        Mul,
        Div,
    }
}
