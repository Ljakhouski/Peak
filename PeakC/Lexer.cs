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
        
        private string filePath;
        private int    lineNumber     =  1;
        private int    positionInLine = -1;
        private int    position       = -1;  // - first char for "while( getChar() )"

        private char[] content;
        private char   ch;

        private string buffer="";
        /*********************/

        private bool   spaceMode = false;
        private bool   quotesMode = false;  //  """"""
        private bool   alternativeQuotesMode = false; // '''''

        public Lexer(string path)
        {
            filePath = path;
            try
            {
                using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
                    content = sr.ReadToEnd().ToCharArray();
            }
            catch (FileLoadException) 
            { 
                Error.FileNotFoundErrMessage(path);
            }
            
            FixIncorrectFileEnd();
        }

        private void FixIncorrectFileEnd()
        {
            while (content.Last() == ' '
                || content.Last() == '\r'
                || content.Last() == '\n'
                || content.Last() == '\t')
                Array.Resize(ref content, content.Length - 2);
        }
        public bool EndOfFile()
        {
            if (!(position < content.Length - 1))
                return true;
            return false;
        }
        
        Token MakeToken(string content)
        {
            var t = new Token(content, filePath, lineNumber, positionInLine);
            buffer = "";
            positionInLine = -1;
            return t;
        }

        Token MakeToken(type type, string content)
        {
            var t = new Token(type, content, filePath, lineNumber, positionInLine);
            buffer = "";
            positionInLine = -1;
            return t;
        }
        private bool nextChar()
        {
            if (!EndOfFile())
            {
                positionInLine++;
                position++;
                ch = content[position];
                return true;
            }
            return false;
        }

        public Token GetToken()
        {
            while (nextChar())

                if ((quotesMode && ch != '"') || (alternativeQuotesMode && ch != "'"[0]))

                    buffer += ch;


                else
                {
                    if (ch == ' ')
                    {
                        if (!spaceMode && buffer.Length > 0)  // if it's the first space-char, then we pack everything that was before it
                            return MakeToken(buffer);
                        // else ignore and parse further
                    }
                    else
                    {
                        spaceMode = false; // when meeting anything except space-char

                        if (ch == '"')
                            if (!quotesMode) // if is the first "
                            {
                                if (buffer.Length > 0) // if the token (before quotes-expression) begun
                                {
                                    position--; // for saving state lexer
                                    return MakeToken(buffer);
                                }


                                quotesMode = true;
                            }
                            else // end of quotes-expression
                            {
                                quotesMode = false;
                                return MakeToken(type.StrConst, buffer);
                            }

                        else if (ch == '\'')
                            if (!alternativeQuotesMode) // if is the first '
                            {
                                if (buffer.Length > 0) // if the token (before quotes-expression) begun
                                {
                                    position--; // for saving state lexer
                                    return MakeToken(buffer);
                                }

                                alternativeQuotesMode = true;
                            }
                            else // end of quotes-expression
                            {
                                alternativeQuotesMode = false;

                                return MakeToken(type.StrConst, buffer);
                            }

                        /*else if (ch == '\n')
                        {
                            positionInLine = -1;
                            lineNumber++;

                            if (quotesMode || alternativeQuotesMode) // saving '\n' only in "title" or 'title'
                                buffer += ch;
                            /*else if (buffer.Length > 0)
                                return newToken(buffer);
                        } */
                        else if (ch == '\r')
                        {

                        }
                        else
                        {
                            //  += -= *= /= != == + - * / . , : ; .........

                            try
                            {
                                string r = String.Concat<char>(content).Substring(position, 2);
                                foreach (string s in sequence)
                                    if (s == String.Concat<char>(content).Substring(position, s.Length))
                                    // if "content " includes "sequence":
                                    {
                                        // separate the buffer from a special sequence:
                                        if (buffer.Length > 0)
                                        {
                                            position--; // for repeat iteration
                                            return MakeToken(buffer);
                                        }

                                        // in next ineration:
                                        var t = MakeToken(s);
                                        position += s.Length;
                                        position--; // because positon is increased, before "ch = content[position]"
                                        return t;
                                    }
                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                // too long sequence
                            }

                            // for single charting:
                            //     "1+2=7"    <=>    1, +, 2, =, 7

                            if (singleCharsSequence.Contains(ch))
                            {
                                if (buffer.Length > 0)
                                {
                                    position--; // for repeat iteration
                                    return MakeToken(buffer);
                                }

                                return MakeToken(ch.ToString());
                            }

                            // default:

                            buffer += ch;
                        }
                    }
                }


            if (buffer.Length > 0)
                return MakeToken(buffer);

            /////////////
            return null;
        }

    }
}
