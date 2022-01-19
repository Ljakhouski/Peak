using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Parser
{
    enum Scope
    {
        Global,
        Local
    }
    class Parser
    {
        private Lexer lexer;
        private Preprocessor preproc;
        private Token t;
        private List<string> loadetFileNames = new List<string>();

        public CodeNode GetNode(string path)
        {
            this.lexer = new Lexer(path);
            this.preproc = new Preprocessor(lexer);
            return parse(Scope.Global);
        }
       
       
        private bool next()
        {
            if (preproc.NextTokenExist())
            {
                t = preproc.GetNextToken();

                return true; 
            }
            return false;
        }

        private Token getNext()
        {
            if (next())
                return t;
            else
                Error.ErrMessage(t, "unfinished expression");
            return null;
        }

        private void setNext()
        {
            if (!next())
                Error.ErrMessage(t, "unfinished expression");
        }
        private void expect(string token)
        {
            if ((next() && t == token) == false)
                Error.ErrMessage(t, "expected \"" + token + '"');
        }

        private void expect(type type)
        {
            if ((next() && t.Type == type) == false)
                Error.ErrMessage(t, "expected " + type.ToString());
        }
        private CodeNode parse(Scope scope)
        {
            var n = new CodeNode(scope);

            while (next())
            {
                if (t == "load")
                {
                    n.Node.Add(parseLoad());
                }
                //else if (t == "if")
                // ...


            }

            return n;
        }

          
        private LoadNode parseLoad()
        {
            if (t == "load")
            {
                expect(type.StrConst);
                var s = t; 
                expect(";"); 
                return new LoadNode(s);
            }
            else
                throw new Exception();
        }
    }
}
