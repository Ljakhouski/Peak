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
        public string  FilePosition  { get; set; }
        public int     LinePosition  { get; set; }
        public int     PositionInLine{ get; set; }

        public Token(type type)
        {
            this.Type = type;
        }
        public Token() { }
        public Token(type type, string content, string filePosition, int linePosition, int position)
        {
            this.Type = type;
            this.Content = content;
            this.FilePosition = filePosition;
            this.LinePosition = linePosition;
            this.PositionInLine = position;
        }

        public Token(type type, string content, Token forMetaInf)
        {
            this.Type = type;
            this.Content = content;
            this.LinePosition = forMetaInf.LinePosition;
            this.PositionInLine = forMetaInf.PositionInLine;
            this.FilePosition = forMetaInf.FilePosition;
        }
        public Token(string content, string filePosition, int linePosition, int position)
        {
            this.Content = content;
            this.FilePosition = filePosition;
            this.LinePosition = linePosition;
            this.PositionInLine = position;
            DefineType();
        }

        private void DefineType()
        {
            bool isIntConst()
            {
                string numbers = "0123456789";
                foreach (char ch in numbers)
                {
                    if (!Content.Contains(ch))
                        return false;
                }
                return true;
            }

            bool isBoolConst()
            {
                if (this.Content == "true" ||
                    this.Content == "false")
                    return true;
                return false;
            }

            string[] keyWords =
            {
                "if"     ,
                "else"   ,
                "while"  ,
                "func"   ,
                "proc"   ,
                "struct" ,
                "load"   ,
                "define" ,
            };

            string[] operators =
            {
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
                 "<<"       ,
                 "break"    ,
                 "continue" ,
            };

            string[] serviceLexems =
            {
                "(", ")", "[", "]", "{", "}",
                "/*"       ,
                "*/"       ,
                "//"       ,
                "int"      ,
                "str"      ,
                "double"   ,
                "bool"     ,
                "array"    ,
                "dict"     ,
                "stack"    ,
            };

            string[] modifiers =
            {
                "native"   ,
                "@export"  ,
            };



            if (isIntConst())
                this.Type = type.IntConst;
            else if (isBoolConst())
                this.Type = type.BoolConst;
            else if (keyWords.Contains(this.Content))
                this.Type = type.KeyWord;
            else if (operators.Contains(this.Content))
                this.Type = type.Operator;
            else if (modifiers.Contains(this.Content))
                this.Type = type.Modifier;
            else if (serviceLexems.Contains(this.Content))
                this.Type = type.ServiceLexem;
            else if (Content == "\n")
                this.Type = type.NextLine;
            else if (Content == ";")
                this.Type = type.NextExpression;
            else
                if (Content.Length!=0)
                this.Type = type.Identifier;
        }
    }

    class RoundBracketExpressionToken : Token
    {
        public List<Token> Expression { get; set; }
        public RoundBracketExpressionToken(List<Token> expression)
        {
            this.Type = type.GroupToken;
            this.Content = "()";

            if (expression.Count == 0)
                this.IsEmpty = true;
            else
            {
                this.LinePosition = expression[0].LinePosition;
                this.FilePosition = expression[0].FilePosition;
              this.PositionInLine = expression[0].    PositionInLine;
            }

            this.Expression = expression;
        }
    }

    class SquareBracketExpressionToken : Token
    {
        public List<Token> Expression { get; set; }
        public SquareBracketExpressionToken(List<Token> expression)
        {
            this.Type = type.GroupToken;
            this.Content = "[]";

            if (expression.Count == 0)
                this.IsEmpty = true;
            else
            {
                this.LinePosition = expression[0].LinePosition;
                this.FilePosition = expression[0].FilePosition;
              this.PositionInLine = expression[0].    PositionInLine;
            }

            this.Expression = expression;
        }
    }
    enum type
    {
        Identifier,
        NextLine,
        NextExpression,

        KeyWord,    // func proc while if 
        Operator,   // +-*/ : += -= ++ -- , 

        BoolConst,
        IntConst,
        DoubleConst,
        StrConst,
        Modifier,

        ServiceLexem,

        /* For other Token-classes that Token extends */

        GroupToken
    }

  /*  enum constType
    {
        

    }*/
}
