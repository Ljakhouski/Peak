using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak
{
    class Preprocessor
    {
        private Lexer l;
        private Token currentToken; // for file-end predicting
        public Preprocessor(Lexer lexer)
        {
            this.l = lexer;
            makeNext();
        }
        public Token GetNextToken() // unsafe, before calling need the file-end-checking using a lexer
        {
            if (currentToken != null)
            {
                Token t = currentToken;
                makeNext();
                return t;
            }
            else
                throw new Exception();
        }
       
        public bool NextTokenExist()
        {
            if (currentToken == null)
                return false;
            return true;
        }
        private void makeNext()
        {
            if (l.EndOfFile())
            {
                currentToken = null;
                return;
            }


            Token t = l.GetToken();

            if (t == "//")
            {
                while (l.EndOfFile() == false)
                {
                    t = l.GetToken();

                    if (t == "\n")
                    {
                        makeNext();
                        return;
                    }
                }
                currentToken = null;
            }
            else if (t == "/*")
            {
                while (l.EndOfFile() == false)
                {
                    t = l.GetToken();

                    if (t == "*/")
                    {
                        makeNext();
                        return;
                    }
                }
                currentToken = null;
            }
            else if (t == "\n")
                makeNext();
            else
                currentToken = t;
        }
    }
}
