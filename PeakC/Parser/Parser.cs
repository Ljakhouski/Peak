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
        private Token t;
        private List<string> loadetFileNames = new List<string>();

        public CodeNode GetNode(string path)
        {
            this.lexer = new Lexer(path);
            return Parse(Scope.Global);
        }
        private bool next___()
        {
            if (!lexer.EndOfFile())
            {
                t = lexer.GetToken();

                return true;
            }
            return false;
        }
        private bool deleteCommentAndCheckFileEnd()
        {
            if (t.Content == "//")
            {
                while (next___() && t.Type != type.NextLine)
                    ;
                return next(); // set token after delete comment, if not end of file
            }
            else if (t.Content == "/*")
            {
                while (next___() && t.Content != "*/")
                    ;
                return next();
            }
            else
                // {
                //      if (!lexer.EndOfFile())
                //  }
                return true;
            //set next token after comment
            

            
        }
        private bool next()
        {
            if (!lexer.EndOfFile())
            {
                t = lexer.GetToken();

                return deleteCommentAndCheckFileEnd(); 
            }
            return false;
        }

        private Token nextToken()
        {
            if (next())
                return t;
            else
                Error.ErrMessage(t, "unfinished expression");
            return null;
        }

        private CodeNode Parse(Scope scope)
        {
            var n = new CodeNode(scope);

            var buffer = new List<Token>();
            var modifiers = new List<Token>();

            while (next())

                if (t.Type == type.NextLine)
                {
                    SaveBufferAsExpression(buffer, n);
                    modifiers = new List<Token>();
                }
                else if (t.Type == type.NextExpression)
                {
                    SaveBufferAsExpression(buffer, n);
                }
                else if (t.Type == type.Modifier)
                {
                    modifiers.Add(t);
                    buffer.Add(t);
                }
                else
                    buffer.Add(t);

            return n;
        }

        private void SaveBufferAsExpression(List<Token> buffer, CodeNode n)
        {
            if (buffer.Count > 0)
                n.Node.Add(ExprParser.GetAst(buffer));
        }
    }
}
