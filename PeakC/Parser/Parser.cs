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
                return position >= 0 ? usedTokens[position] : null;
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
                throw new CompileException();
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
                case NonterminalType.Args:
                    return parseArgs();
                case NonterminalType.MethodCall:
                    return parseMethodCall();
                case NonterminalType.WordOperator:
                    return parseWordOperator();
                case NonterminalType.Data:
                    return parseData();
                default:
                    if (nt.IsBinary)
                        return parseBinary(nt);
                    break;

            }

            throw new CompileException();
        }

        private LoadNode parseLoad()
        {
            if (t == "load")
            {
                var metaInf = t;
                expect(type.StrValue);
                var s = t;
                expect(";");
                return new LoadNode(s) { MetaInf = metaInf };
            }
            else
                throw new CompileException();
        }

        private CodeBlockNode parseCodeBlock()
        {
            var n = new CodeBlockNode();

            while (true)

                if (!nextExist())
                    return n;

                else if (getNext() == "]")
                    return n;

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
            var modifier = parse(NonterminalType.Modifier);

            ModifierNode getModifier()
            {
                if (modifier is ModifierNode)
                    return modifier as ModifierNode;
                else if (modifier is EmptyNode)
                    return null;
                else
                    throw new CompileException();
            }

            if (getNext() == "#")
            {
                next();
                var name = expectName();
                if (getNext() == "<-")
                {
                    next();
                    var rightExpression = parse(NonterminalPreority.GetNextByPreority(NonterminalType.Assignment));
                    expect(";");
                    return new VariableInitNode(name, rightExpression);
                }
                else
                {
                    expect(";");
                    return new VariableInitNode(name);
                }
            }
            else if (getNext() == "import")
            {
                next();
                if (getNext() == "proc" || getNext() == "func")
                {
                    var node = parseMethod();
                    if (node.Code == null)
                    {
                        expect("from");
                        expect(type.StrValue);
                        node.DllPath = t;
                        expect(";");
                        return node;
                    }
                    else
                        Error.ErrMessage(node.Code.MetaInf, "method code not expected");
                }
                else
                    Error.ErrMessage(t, "\"func\" or \"proc\" expected");
            }
            else if (getNext() == "proc" || getNext() == "func")
            {
                return parseMethod();
            }
            else if (getNext() == "if")
            {
                return parseIf().ConvertToIfNode();
                
            }
            else if (getNext() == "while")
            {
                next();
                var metaInfToken = t;

                var condition = parse(NonterminalType.AndOr);
                expect("[");
                var code = parse(NonterminalType.CodeBlock) as CodeBlockNode;
                expect("]");
                return new WhileNode() { Condition = condition, Code = code, MetaInf = metaInfToken };
            }
            else if (getNext().Type == type.WordOperator)
            {
                return parseWordOperator();
            }
            else
            {
                //var modifier = parse(NonterminalType.Modifier);

                var expr = parse(NonterminalType.Assignment); // if begin <type_expr> then parse as var-declaration, else expression will be <expression> <;>
                if (maybeTypeExpression(expr))
                {
                    var varInitNode = new VariableInitNode(
                        expr,
                        expectName()
                        );

                    varInitNode.Modifiers = modifier.Modifiers;

                    if (getNext() == "<-")
                    {
                        next();
                        varInitNode.RightExpression = parse(NonterminalPreority.GetNextByPreority(NonterminalType.Sequence));
                        expect(";");
                        return varInitNode;
                    }
                    else
                    {
                        expect(";");
                        return varInitNode;
                    }
                    //Error.ErrMessage(getNext(), "expected \"<<\"");

                }
                else
                {
                    if (modifier is EmptyNode == false)
                    {
                        Error.ErrMessage(modifier.Modifiers[0], "expected func/proc/variable declaration");
                    }
                    expect(";");
                    return expr;
                }
            }
            throw new CompileException();
        }

        private IfElifNode parseIf()
        {
            expect("if");
            var metaInfToken = t;

            var condition = parse(NonterminalType.AndOr);
            expect("[");
            var code = parse(NonterminalType.CodeBlock) as CodeBlockNode;
            expect("]");

            var node = new IfElifNode() { Condition = condition, IfTrueCode = code, MetaInf = metaInfToken };


            const bool else_if_Sequence = true;
            while (else_if_Sequence)
            {
                if (getNext() == "else")
                {
                    expect("else");
                    if (getNext() == "if")
                    {
                        var elifCondition = parse(NonterminalType.AndOr);
                        expect("[");
                        var elifCode = parse(NonterminalType.CodeBlock) as CodeBlockNode;
                        expect("]");
                        node.ElseIfNodes.Add(new IfNode() { Condition = elifCondition, IfTrueCode = elifCode });
                    }
                    else
                    {
                        parseElse();
                        return node;
                    }
                }
                else if (getNext() == "elif")
                {
                    var elifCondition = parse(NonterminalType.AndOr);
                    expect("[");
                    var elifCode = parse(NonterminalType.CodeBlock) as CodeBlockNode;
                    expect("]");
                    node.ElseIfNodes.Add(new IfNode() { Condition = elifCondition, IfTrueCode = elifCode });
                }
                else
                    break;
            }

            if (getNext() == "else")
            {
                parseElse();
                return node;
            }
            return node;

            void parseElse()
            {
                next();
                if (getNext() == "[")
                {
                    expect("[");
                    var elseCode = parse(NonterminalType.CodeBlock) as CodeBlockNode;
                    expect("]");

                    node.ElseCode = elseCode;
                }
                else
                    Error.ErrMessage(t, "[] expected");
            }
        }
        private MethodNode parseMethod()
        {
            if (getNext() == "func")
                return parseFunc();
            else if (getNext() == "proc")
                return parseProc();
            else
                throw new CompileException();
        }
        private MethodNode parseProc()
        {
            expect("proc");
            var name = expectName();
            expect("(");
            var args = parse(NonterminalType.Args);
            expect(")");

            if (getNext() == "[")
            {
                next();
                var bn = parse(NonterminalType.CodeBlock);
                expect("]");
                return new MethodNode(name, args, retType: null, (CodeBlockNode)bn);
            }
            else
                return new MethodNode(name, args, retType: null);
            Error.ErrMessage(getNext(), "expected \";\" or \"[]\"");


        }

        private MethodNode parseFunc()
        {
            expect("func");
            expect("(");
            var retType = parse(NonterminalType.Data);
            if (maybeTypeExpression(retType) == false) Error.ErrMessage(t, "expected type expression");
            expect(")");

            var name = expectName();

            expect("(");
            var args = parse(NonterminalType.Args);
            expect(")");

            if (getNext() == ";")
            {
                next();
                return new MethodNode(name, args, retType);
            }
            else if (getNext() == "[")
            {
                next();
                var block = parse(NonterminalType.CodeBlock) as CodeBlockNode;
                expect("]");
                return new MethodNode(name, args, retType, block);
            }
            else
                Error.ErrMessage(getNext(), "expected \";\" or \"[]\"");
                throw new CompileException();
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
            Node n = parse(NonterminalPreority.GetNextByPreority(NonterminalType.AndOr));

            if (getNext() == ",") // if expression contains at least one comma, then <dotExpr> -> <expr> <,> <expr> ... ? 
            {
                var sequence = new SequenceNode() { MetaInf = t };
                sequence.Sequence.Add(n);

                while (getNext() == ",") // if expression contains two and more comma. <sequence> -> <expr> { <,> <expr> }
                {
                    next();
                    var expr = parse(NonterminalPreority.GetNextByPreority(NonterminalType.AndOr));
                    if (expr is EmptyNode)
                        Error.ErrMessage(t, "expression expected");
                    else
                        sequence.Sequence.Add(expr);

                }
                return sequence;

            }
            else
                return n;


        }
        private Node parseBinary(Nonterminal nonterm) // <binaryEpxr> -> <binaryEpxr> operator <expr> | <expr> 
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
        private Node parseMethodCall() // <expression> -> <name> + '(' + <expression> + ')' | <expression>
        {
            var expr = parseData();
            if (expr is IdentifierNode && getNext() == "(")
            {
                next();
                if (getNext() == ")")
                {
                    next();
                    return new MethodCallNode((expr as IdentifierNode).Id);
                }
                else
                {
                    var n = new MethodCallNode((expr as IdentifierNode).Id, parse(NonterminalType.Sequence));
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
            else if (t.Type == type.StrValue) return new ConstValueNode(t);
            else if (t.Type == type.BoolValue) return new ConstValueNode(t);
            else if (t.Type == type.Identifier) return new IdentifierNode(t);
            /*{
                if (getNext() == "(")
                {
                    back();
                    return parse(NonterminalType.MethodCall);
                }
                else
                    
            }*/
            //else if (t.Type == type.Term       ) return new IdentifierNode(t);
            else
                Error.ErrMessage(t, "identifier expected");

            throw new CompileException();
        }

        private Node parseWordOperator()
        {
            next();

            var node = new WordOperatorNode(t);
            node.MetaInf = t;

            node.Expression = parse(NonterminalType.AndOr);
            expect(";");
            return node;
        }

        private Node parseArgs()
        {
            if (getNext() == ")")
                return new EmptyNode(t);

            var n = new SequenceNode();
            var expr = parse(NonterminalType.Assignment);

            if (maybeTypeExpression(expr))
            {
                var name = expectName();
                if (getNext() == "<-")
                {
                    // var varInit = parse(NonterminalType.)
                    Error.ErrMessage(nextToken(), "assigment is not support for argument");
                }
                n.Sequence.Add(new VariableInitNode(expr, name));
            }
            else
                Error.ErrMessage(t, "type expression expected");


            while (true)
                if (getNext() == ",")
                {
                    next();
                    var nextExpr = parse(NonterminalType.Assignment);
                    if (maybeTypeExpression(nextExpr))
                    {
                        var name = expectName();
                        n.Sequence.Add(new VariableInitNode(nextExpr, name));
                    }
                    else
                        Error.ErrMessage(t, "type expression expected");
                }
                else
                    return n;

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
            return false;
        }

    }
}
