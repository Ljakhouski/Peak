using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peak.PeakC
{
    class Lexer
    {
        private readonly char[] singleCharsSequence = new char[]
        {
            '+', '-', '*', '/', '!', '=',
            '<', '>', '[', ']', '{', '}',
                      '(', ')',
                  '.', ',', ';', ':',
            '?',
            '\n'
        };

        private readonly string[] sequence = new string[]
        {
            "+=", "-=", "*=", "/=", "!=",
            "++", "--", "**", "//", "/*", "*/",
            "<=", ">=", "<<", ">>" 
            // add your tokens only in increasing length!
            // "alpha+=1"  <=>  alpha, +=, 1
        };
        
        private string FilePath;
        private int    LineNumber     = 1;
        private int    PositionInLine = -1;
        private int    Position       = -1;  // - first char for "while( getChar() )"

        private char[] Content;
        private char   ch;

        private string Buffer;
        /*********************/

        private bool   spaceMode = false;
        private bool   quotesMode = false;  //  """"""
        private bool   alternativeQuotesMode = false; // '''''

        public Lexer(string path)
        {
            FilePath = path;
            try
            {
                using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                    Content = sr.ReadToEnd().ToCharArray();
            }
            catch (FileLoadException) 
            { 
                Error.FileNotFoundErrMessage(path);
            }
            
            FixIncorrectFileEnd();
        }

        private void FixIncorrectFileEnd()
        {
            while (Content.Last() == ' '
                || Content.Last() == '\r'
                || Content.Last() == '\n'
                || Content.Last() == '\t')
                Array.Resize(ref Content, Content.Length - 2);
        }
        public bool EndOfFile()
        {
            if (!(Position < Content.Length - 1))
                return true;
            return false;
        }
        private bool NextChar()
        {
            if (!EndOfFile())
            {
                PositionInLine++;
                Position++;
                ch = Content[Position];
                return true;
            }
            return false;
        }

        Token MakeToken(string Content)
        {
            var t = new Token(Content, FilePath, LineNumber, PositionInLine);
            Buffer = "";
            PositionInLine = -1;
            return t;
        }

        Token MakeToken(type type, string Content)
        {
            var t = new Token(type, Content, FilePath, LineNumber, PositionInLine);
            Buffer = "";
            PositionInLine = -1;
            return t;
        }

        public Token GetToken()
        {
            while (NextChar())

                if ((quotesMode && ch != '"') || (alternativeQuotesMode && ch != "'"[0]))

                    Buffer += ch;


                else
                {
                    if (ch == ' ')
                    {
                        if (!spaceMode && Buffer.Length > 0)  // если пробел встречается первый раз, то упаковываем все то что было до него
                            return MakeToken(Buffer);
                        // иначе игнорируем и парсим дальше
                    }
                    else
                    {
                        spaceMode = false; // при встрече с чем угодно кроме пробела

                        if (ch == '"')
                            if (!quotesMode) // если встретилось впервые
                            {
                                if (Buffer.Length > 0) // если токен (до выражения в кавычках) уже начат
                                {
                                    Position--; // for saving state lexer
                                    return MakeToken(Buffer);
                                }


                                quotesMode = true;
                            }
                            else // конец выражения в кавычках
                            {
                                quotesMode = false;
                                return MakeToken(type.StrConst, Buffer);
                            }

                        else if (ch == '\'')
                            if (!alternativeQuotesMode) // если встретилось впервые
                            {
                                if (Buffer.Length > 0) // если токен (до выражения в кавычках) уже начат
                                {
                                    Position--; // for saving state lexer
                                    return MakeToken(Buffer);
                                }

                                alternativeQuotesMode = true;
                            }
                            else // конец выражения в кавычках
                            {
                                alternativeQuotesMode = false;

                                return MakeToken(type.StrConst, Buffer);
                            }

                        else if (ch == '\n')
                        {
                            PositionInLine = -1;
                            LineNumber++;

                            if (quotesMode || alternativeQuotesMode) // saving '\n' only in "title" or 'title'
                                Buffer += ch;
                            /*else if (Buffer.Length > 0)
                                return newToken(Buffer);*/
                        }
                        else if (ch == '\r')
                        {

                        }
                        else
                        {
                            //  += -= *= /= != == + - * / . , : ; .........

                            try
                            {
                                string r = String.Concat<char>(Content).Substring(Position, 2);
                                foreach (string s in sequence)
                                    if (s == String.Concat<char>(Content).Substring(Position, s.Length))
                                    // if "Content " includes "sequence":
                                    {
                                        // separate the Buffer from a special sequence:
                                        if (Buffer.Length > 0)
                                        {
                                            Position--; // for repeat iteration
                                            return MakeToken(Buffer);
                                        }

                                        // in next ineration:
                                        var t = MakeToken(s);
                                        Position += s.Length;
                                        Position--; // because posion is increased, before "ch = Content[Position]"
                                        return t;
                                    }
                            }
                            catch (ArgumentOutOfRangeException e)
                            {

                            }



                            // for single charting:
                            //     "1+2=7"    <=>    1, +, 2, =, 7

                            

                            if (singleCharsSequence.Contains(ch))
                            {
                                if (Buffer.Length > 0)
                                {
                                    Position--; // for repeat iteration
                                    return MakeToken(Buffer);
                                }


                                return MakeToken(ch.ToString());
                            }



                            // DEFAULT:

                            Buffer += ch;
                        }
                    }
                }


            if (Buffer.Length > 0)
                return MakeToken(Buffer);

            /////////////
            return null;
        }

    }
}
