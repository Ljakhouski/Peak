using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC.Parser;

namespace Peak.PeakC
{
    class Node
    {
        public Token MetaInf { get; set; } // if Node is empty, becos need for error message filePosition, linePosition and position
        public List<Token> Modifiers { get; set; } = new List<Token>();
        public Node() { }
    }

    class CodeNode : Node
    {
        public Scope Scope { get; set; }
        public List<Node> Node { get; set; } = new List<Node>();
        public Token ContextName { get; set; } // for import files as variable

        public CodeNode(Scope scope)
        {
            this.Scope = scope;
        }
    }
}
