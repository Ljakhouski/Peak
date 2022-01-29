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
        /* Common LL(k=1) parser */

        private Lexer lexer;
        private Preprocessor preproc;
        
        private List<Token> usedTokens = new List<Token>();
        int position = -1;
        private Token t 
        { 
            get 
            {
                return position>=0? usedTokens[position] : null; 
            } 
        }

        private List<string> loadetFileNames = new List<string>();
        public ProgramNode GetNode(string path)
        {
            this.lexer = new Lexer(path);
            this.preproc = new Preprocessor(lexer);
            return (ProgramNode)parse(NonterminalType.Program);
        }
        private bool next()
        {
            if (position == usedTokens.Count - 1)
            {
                if (preproc.NextTokenExist())
                {
                    usedTokens.Add(preproc.GetNextToken());
                    position++;
                    return true;
                }
                else
                    return false;
            }
            else if (position < usedTokens.Count - 1)
            {
                position++;
                return true;
            }
            else
                throw new Exception();
        }

        // set next Token and return it
        private Token nextToken()
        {
            if (next())
                return t;
            else
                Error.ErrMessage(t, "unfinished expression");
            return null;
        }

        private Token getNext()
        {
            if (next())
            {
                var rt = t;
                back();
                return rt;
            }
            else
                return new Token(type.FileEnd, "", t.File, t.Line, t.Position);
        }

        private bool nextExist()
        {
            return preproc.NextTokenExist();
        }
        private void back()
        {
            position--;
        }
        private void back(int backPosition)
        {
            position = backPosition;
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
        private Token expectName()
        {
            if ((next() && t.Type == type.Identifier) == false)
                Error.ErrMessage(t, "expected name");
            
            return t;
        }

        /* parsing methods */

        private ProgramNode parseProgram()
        {
            var programNode = new ProgramNode();
            while (true)
            {
                if (next())
                    if (t == "load")
                    {
                        programNode.Node.Add(parseLoad());

                        if (getNext() == ";")
                            next();
                    }
                    else
                    {
                        back();
                        break;
                    }
                else
                    return programNode;
                    
            }
            programNode.Node.Add(parse(NonterminalType.CodeBlock));
            if (next())
                Error.ErrMessage(t, "expected end of file");

            return programNode;
        }

        private Node parse(NonterminalType type)
        {
            return parse(NonterminalPreority.GetByType(type));
        }
        private Node parse(Nonterminal nt)
        {
            switch (nt.Type)
            {
                case NonterminalType.Program: // contains <load "...">
                    return parseProgram();                    
                case NonterminalType.CodeBlock:
                    return parseCodeBlock();
                case NonterminalType.Modifier:
                    return parseModifier();
                case NonterminalType.Dot:
                    return parseDot();
                case NonterminalType.Sequence:
                    return parseSequence();
                case NonterminalType.Data:
                    return parseData();
                default:
                    if (nt.IsBinary)
                        return parseBinry(nt);
                    break;
                    
            }
            
            throw new Exception();
        }
          
        private LoadNode parseLoad()
        {
            if (t == "load")
            {
                expect(type.StrValue);
                var s = t; 
                expect(";"); 
                return new LoadNode(s);
            }
            else
                throw new Exception();
        }

        private CodeBlockNode parseCodeBlock()
        {
            var n = new CodeBlockNode();

            while (true)

                if (!nextExist())
                    return n;
                
                else if (getNext() == "]")
                {
                    next();
                    return n;
                }
                else
                    n.Node.Add(parseCodeBlockExpression());       
        }
        private Node parseCodeBlockExpression()
        {
            /* parsing if-else-while-proc-func-varInit 
                  first parse if-else-while. Is native LL(1) rules
             */
            /* for other grammar: parse any expression, which is more in preority, because he can is <type_expression>
             
            check expression, is type-expression?:
                if yes, then parse variable-declaration or something else
                if not, then expression is just increment/decrement/func-call/assigment/..., then expect ';'

             */


            /* <expression> | <type_expression> <name> '<<' <expression> ';' | <type_expression> <name> ...*/
            
            //int state = position;
            

            if(getNext() == "#")
            {
                next();
                var name = expectName();
                if (getNext() == "<<")
                {
                    next();
                    var rightExpression = parse(NonterminalPreority.GetNextByPreority(NonterminalType.Assigment));
                    expect(";");
                    return new VariableInitNode(name, rightExpression);
                }
                else
                {
                    expect(";");
                    return new VariableInitNode(name);
                }
            }
            else if (getNext() == "proc")
            {
                next();
                var name = expectName();
                expect("(");
                var args = parse(NonterminalType.Sequence);
                if (args is SequenceNode || args is VariableInitNode || args is EmptyNode)
                {
                    expect(")");
                    if (getNext() == ";")
                    {
                        next();
                        return new ProcedureNode(name, args);
                    }
                    else if (getNext() == "[")
                    {
                        next();
                        var bn = parse(NonterminalType.CodeBlock);
                        expect("]");
                        return new ProcedureNode(name, args, (CodeBlockNode)bn);
                    }
                    else
                        Error.ErrMessage(getNext(), "expected \";\" or \"[]\"");
                }
                else
                    Error.ErrMessage(t, "wrong argument expression");
            }
            else
            {
                var modifier = parse(NonterminalType.Modifier);

                var expr = parse(NonterminalType.Dot); // if begin <type_expr> then parse as var-declaration, else expression will be <expression> <;>
                if (maybeTypeExpression(expr))
                {
                    var varInitNode = new VariableInitNode(
                        expectName()
                    );

                    varInitNode.Modifiers = modifier.Modifiers;

                    if (getNext() == "<<")
                    {
                        next();
                        varInitNode.RightExpression = parse(NonterminalPreority.GetNextByPreority(NonterminalType.CodeBlock));
                        expect(";");
                        return varInitNode;
                    }
                    else
                        Error.ErrMessage(getNext(), "expected \"<<\"");

                }
                else
                {
                    if (modifier is EmptyNode == false)
                    {
                        Error.ErrMessage(modifier.Modifiers[0], "expected name");
                    }
                    expect(";");
                    return expr;
                }
            }
            throw new Exception();
        }

        private Node parseModifier()
        {
            var mn = new ModifierNode();
            while (getNext().Type == type.Modifier)
                mn.Modifiers.Add(nextToken());

            if (mn.Modifiers.Count == 0)
                return new EmptyNode();
            else
                return mn;
            
        }
        private Node parseDot()
        {
            var dotNode = new DotNode() { Sequence = new List<Node>() };
            Node n = parse(NonterminalPreority.GetNextByPreority(NonterminalType.Dot));

            if (getNext() == ".") // if expression contains at least one dot, then <dotExpr> -> <expr> <.> <expr> ... ? 
            {
                next();
                dotNode.Sequence.Add(n);
                var expr = parse(NonterminalPreority.GetNextByPreority(NonterminalType.Dot));
                if (expr is EmptyNode)
                    Error.ErrMessage(t, "expression expected");
                else
                    dotNode.Sequence.Add(expr);
            }
            return n;

            while (true) // if expression contains two and more dot. <dotExpr> -> <expr> { <.> <expr> }
            {
                if (getNext() == ".")
                {
                    next();
                    var expr = parse(NonterminalPreority.GetNextByPreority(NonterminalType.Dot));
                    if (expr is EmptyNode)
                        Error.ErrMessage(t, "expression expected");
                    else
                        dotNode.Sequence.Add(expr);
                }
                else
                    return n;
            }
        }
        
        private Node parseSequence()
        {
            var sequence = new SequenceNode() { Sequence = new List<Node>() };
            Node n = parse(NonterminalPreority.GetNextByPreority(NonterminalType.AndOr));

            if (getNext() == ",") // if expression contains at least one comma, then <dotExpr> -> <expr> <,> <expr> ... ? 
            {
                next();
                sequence.Sequence.Add(n);
                var expr = parse(NonterminalPreority.GetNextByPreority(NonterminalType.AndOr));
                if (expr is EmptyNode)
                    Error.ErrMessage(t, "expression expected");
                else
                    sequence.Sequence.Add(expr);
            }
            return n;

            while (true) // if expression contains two and more comma. <sequence> -> <expr> { <,> <expr> }
            {
                if (getNext() == ",")
                {
                    next();
                    var expr = parse(NonterminalPreority.GetNextByPreority(NonterminalType.AndOr));
                    if (expr is EmptyNode)
                        Error.ErrMessage(t, "expression expected");
                    else
                        sequence.Sequence.Add(expr);
                }
                else
                    return n;
            }
        }
        private Node parseBinry(Nonterminal nonterm) // <binaryEpxr> -> <binaryEpxr> operator <expr> | <expr> 
        {
            Node n;
            
            n = parse(NonterminalPreority.GetNextByPreority(nonterm));
            while (true)
            {
                if (nonterm.Terminals.Contains(getNext().Content))
                {
                    next();
                    var binaryOperator = t;
                    var nextExpr = parse(NonterminalPreority.GetNextByPreority(nonterm));
                    if (nextExpr is EmptyNode)
                        Error.ErrMessage(t, "expression expected");
                    else
                    {
                        n = new BinaryNode(binaryOperator, n, nextExpr);
                    }
                }
                else
                    return n;
            }
            
        }
        private Node parseFuncCall() // <expression> -> <name> + '(' + <expression> + ')' | <expression>
        {
            var expr = parseData();
            if (expr is IdentifierNode && getNext() == "(")
            {
                next();
                if (getNext() == ")")
                {
                    next();
                    return new FuncCallNode((expr as IdentifierNode).Id);
                }
                else
                {
                    next();
                    var n = new FuncCallNode((expr as IdentifierNode).Id, parse(NonterminalType.AndOr));
                    expect(")");
                    return n;
                }
                   
            }
            return expr;
                
        }
        private Node parseData()
        {
            next();

            if (t.Type == type.IntValue)
            {
                if (getNext() == ".")
                {
                    var doubleBeginToken = t;
                    next();
                    if (nextToken().Type == type.IntValue)
                        return new ConstValueNode(new Token(type.DoubleValue, doubleBeginToken.Content + "." + t.Content, doubleBeginToken));

                    else
                        Error.ErrMessage(t, "value expected");
                }
                else
                    return new ConstValueNode(t);
            }
            else if (t.Type == type.StrValue   ) return new ConstValueNode(t);
            else if (t.Type == type.BoolValue  ) return new ConstValueNode(t);
            else if (t.Type == type.Identifier ) return new IdentifierNode(t);
            else
                Error.ErrMessage(t, "identifier expected");

            throw new Exception();           
        }
       
       
        private bool maybeTypeExpression(Node expr)
        {
            if (expr is IdentifierNode)
                return true;
            else if (expr is DotNode)
                foreach (Node n in (expr as DotNode).Sequence)
                {
                    if (!(n is IdentifierNode || n is TypeNode))
                    {
                        return false;
                    }
                }
            return true;
        }
    }
}
