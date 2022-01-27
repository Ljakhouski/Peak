using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC.Parser;

namespace Peak.PeakC
{
    class Node
    {
        public Token MetaInf { get; set; } // (OLD) if Node is empty, because it's need for error message filePosition, linePosition and position
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
        public Node RightExpression { get; set; }
        public VariableInitNode(Token name, Node rightExpression)
        {
            this.Name = name;
            RightExpression = rightExpression;
        }
        public VariableInitNode(Token name)
        {
            this.Name = name;
        }
    }
    class TypeNode : Node
    {
        public Node Type { get; set; }
    }

    class ModifierNode : Node
    {
        public Node Type { get; set; }
    }
    class DotNode : Node
    {
        public List<Node> Sequence { get; set; }
        public DotNode()
        {}
    }

    class BinaryNode : Node
    {
        public Token Operator { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public BinaryNode(Token binaryOperator, Node left=null, Node right=null)
        {
            this.Operator = binaryOperator;
            this.Left = left;
            this.Right = right;
        }
    }
    class ProgramNode : Node
    {
        public List<Node> Node { get; set; } = new List<Node>();
        public Token ContextName { get; set; } // for import files as variable

        public ProgramNode()
        {
            
        }
    }

    class CodeBlockNode : Node
    {
        public List<Node> Node { get; set; } = new List<Node>();
        public Token ContextName { get; set; } // for import files as variable

        public CodeBlockNode()
        {
            
        }
    }
    class ProcedureNode : Node
    {
        public Node Args { get; set; }
        public Token Name { get; set; } 
        public CodeBlockNode Code { get; set; }
        public ProcedureNode(Token name, Node args, CodeBlockNode code = null)
        {
            this.Name = name;
            this.Args = args;
            this.Code = code;
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
