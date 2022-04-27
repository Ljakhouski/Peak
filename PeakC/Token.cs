using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peak.PeakC
{
    class Token
    {
        public bool    IsEmpty       { get; set; }
        public type    Type          { get; set; }
        public string  Content       { get; set; }
        public string  File          { get; set; }
        public int     Line          { get; set; }
        public int     Position      { get; set; }

        public Token(type type)
        {
            this.Type = type;
        }
        public Token() { }
        public Token(type type, string content, string filePosition, int linePosition, int position)
        {
            this.Type = type;
            this.Content = content;
            this.File = filePosition;
            this.Line = linePosition;
            this.Position = position;
        }

        public Token(type type, string content, Token forMetaInf)
        {
            this.Type = type;
            this.Content = content;
            this.Line = forMetaInf.Line;
            this.Position = forMetaInf.Position;
            this.File = forMetaInf.File;
        }
        public Token(string content, string filePosition, int linePosition, int position)
        {
            this.Content = content;
            this.File = filePosition;
            this.Line = linePosition;
            this.Position = position;
            DefineType();
        }
        public static bool operator ==(Token t1, string value)
        {
            if (value == null && (object)t1 == null)
                return true;
            if (t1.Content == value)
                return true;
            return false;
        }
        public static bool operator !=(Token t1, string value)
        {
            if (value == null && (object)t1 == null)
                return false;
            if (t1 == value)
                return false;
            return true;
        }

        private void DefineType()
        {
            bool isIntValue()
            {
                string numbers = "0123456789";
                foreach (char ch in Content)
                {
                    if (!numbers.Contains(ch))
                        return false;
                }
                return true;
            }

         

         
            string[] terms =
            {
                "if"     ,
                "else"   ,
                "while"  ,
                "func"   ,
                "proc"   ,
                "struct" ,
                "load"   ,
                "define" ,
                 "#"        ,
                 "$"        ,
                 "?"        ,
                 "+"        ,
                 "-"        ,
                 "*"        ,
                 "/"        ,
                 "++"       ,
                 "--"       ,
                 "and"      ,
                 "or"       ,
                 "!"        ,
                 "="        ,
                 "!="       ,
                 "<"        ,
                 ">"        ,
                 ">="       ,
                 "<="       ,
                 ":"        ,
                 "."        ,
                 ","        ,
                 ";"        ,
                 "<<"       ,
                 "break"    ,
                 "continue" ,
                 "(", ")", "[", "]", "{", "}",
                 "/*"       ,
                 "*/"       ,
                 "//"       ,
                 //"int"      ,
                 //"str"      ,
                 //"double"   ,
                 //"bool"     ,
                 "array"    ,
                 "dict"     ,
                 "stack"    ,
             //    "native"   ,
             //    "export"   ,
                 "true"     ,
                 "false"    ,
            };

            string[] modifiers =
            {
                "native"   ,
                "export"   ,
            };

            string[] wordOperators =
            {
                "return",
                "break",
                "continue"
            };

            // if (isIntConst())
            //     this.Type = type.IntConst; 
            // else if (isBoolConst())
            //     this.Type = type.BoolConst;
            // else if (terms.Contains(this.Content))
            //     this.Type = type.Terminal;
            // else if (operators.Contains(this.Content))
            //     this.Type = type.Operator;
            // else if (modifiers.Contains(this.Content))
            //     this.Type = type.Modifier;
            // else if (serviceLexems.Contains(this.Content))
            //     this.Type = type.ServiceLexem;
            // else if (Content == "\n")
            //     this.Type = type.NextLine;
            // else if (Content == ";")
            //     this.Type = type.NextExpression;
            if (this.Content == "true" || this.Content == "false")
                this.Type = type.BoolValue;
            else if (wordOperators.Contains(this.Content))
                this.Type = type.WordOperator;
            else if (terms.Contains(this.Content))
                this.Type = type.Term;
            else if (modifiers.Contains(this.Content))
                this.Type = type.Modifier;
            else if (isIntValue())
                this.Type = type.IntValue;
            else if (Content.Length != 0)
                this.Type = type.Identifier;
            else
                Error.ErrMessage(this, "unknow type");
        }
    }

    enum type
    {
        Term,
        Identifier,
        //NextLine,
        //NextExpression,
        //
        //KeyWord,    // func proc while if 
        //Operator,   // +-*/ : += -= ++ -- , 
        //
        StrValue,
        IntValue,
        DoubleValue,
        BoolValue,
        Modifier,
        //
        //ServiceLexem,
        //
        ///* For other Token-classes that Token extends */
        //
        //GroupToken
        FileEnd, // for comparison if the next token is requested
        WordOperator,
    }

  /*  enum constType
    {
        

    }*/
}
