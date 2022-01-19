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

    class EmptyNode : Node
    {
        public EmptyNode(Token t)
        {
            this.MetaInf = t;
        }
        public EmptyNode()
        { }
    }

    class ConstantNode : Node
    {
        public Token Content { get; set; }
        public ConstantNode(Token content)
        {
            this.Content = content;
        }
    }

    class ConstantInitNode: Node
    {
        public Token Name { get; set; }
        public ConstantInitNode(Token name)
        {
            this.Name = name;
        }
    }

    class VariableInitNode : Node
    {
        public Token Name { get; set; }
        public VariableInitNode(Token name)
        {
            this.Name = name;
        }
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

    class SequenceNode : Node
    {
        public List<Node> Sequence { get; set; } = new List<Node>();
        public SequenceNode(List<Node> sequence)
        {
            this.Sequence = sequence;
        }
        public SequenceNode() { }

    }

    class OperatorNode : Node
    {
        public Token OperatorName { get; set; }
        public Node LeftExpression { get; set; }
        public Node RightExpression { get; set; }
        string op()
        {
            return OperatorName.Content;
        }
        public OperatorNode(Token op, Node left, Node right)
        {
            this.OperatorName = op;
            this.LeftExpression = left;
            this.RightExpression = right;
        }
        public OperatorNode(Token op)
        {
            this.OperatorName = op;
        }
    }

    class LoadNode : Node
    {
        public Token LoadFileName { get; set; }

        public LoadNode (Token loadFileName)
        {
            this.LoadFileName = loadFileName;
        }
    }
    class NameNode : Node
    {
        public Token Name { get; set; }
        public NameNode(Token name)
        {
            this.Name = name;
        }
    }
}
