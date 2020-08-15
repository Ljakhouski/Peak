using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peak.PeakC.Parser
{
    class ExprParser
    {
        private List<Token> Expression = new List<Token>();
        private int Position = 0;
        private Token ThisToken;
        private Node FinishedNode;


        public static Node GetAst(List<Token> Expression)
        {
            ExprParser e = new ExprParser();
            return e.GenerateAst(Expression); ;
        }

        private Node GenerateAst(List<Token> expr)
        {
            if (expr.Count == 0)
                return new EmptyNode();

            this.Expression = expr;
            MakeDouble();
            MakeBracketPreority();
            return GenerateOperatorAst();
        }

        private bool isContains(string[] CurrentPreorityLexeme)
        {
            setToEndPosition();

            do
            {
                if (CurrentPreorityLexeme.Contains(ThisToken.Content))
                    return true;
            } while (BeforeToken());

            return false;
        }
        private Node GenerateOperatorAst()
        {
            foreach (string[] l in OperatorPreority.Preority)
            {
                if (isContains(l))
                {
                    if (l[0] == ",")
                        return GetCommaNode();
                  //  else if (l[0] == "[]")
                  //      return getAccesingIndexNode();
                    else
                        return GetOperatorNode(ThisToken.Content); // ThisToken in current state has a operator for parsing
                }
            }

            if (IsVariableInit())
                return FinishedNode;
          //  else if (isDataTypeAndVarName()) // int i
          //      return FinishedNode;
           // else if (isMethodCallNode()) // sin()
           //     return FinishedNode;
            else if (IsOnlyName())
                return FinishedNode;
            else if (IsOnlyConstant())
                return FinishedNode;
          //  else if (isOnlyBracket())
           //     return FinishedNode;
            else
                Error.ErrMessage(ThisToken, "wrong expression");
            return null;
        }

        private bool NextToken()
        {
            if (Position < Expression.Count - 1)
            {
                Position++;
                ThisToken = Expression[Position];
                return true;
            }
            return false;
        }
        private bool BeforeToken()
        {
            if (Position > 0)
            {
                Position--;
                ThisToken = Expression[Position];
                return true;
            }
            return false;
        }
        private List<Token> GetLeftExpression()
        {
            // method "Remove" in list<T> DELETES(!!!) objects in memory
            List<Token> l = new List<Token>();
            for (int i = 0; i < Position; i++)
                l.Add(Expression[i]);
            return l;
        }
        private List<Token> GetRightExpression()
        {
            List<Token> l = new List<Token>();
            for (int i = Position + 1; i < Expression.Count; i++)
                l.Add(Expression[i]);
            return l;
        }

        private void ResetPosition()
        {
            Position = 0;
            ThisToken = Expression[Position];
        }
        private void setToEndPosition() // for "do {} while (BeforeToken())"
        {
            Position = Expression.Count - 1;
            ThisToken = Expression[Position];
        }

        Node GetOperatorNode(string operator_)
        {
            setToEndPosition();

            do
            {
                if (ThisToken.Content == operator_)
                    return new OperatorNode(ThisToken,
                             GetAst(GetLeftExpression ()),
                             GetAst(GetRightExpression())
                             );

            } while (BeforeToken());

            return null;
        }
        private Node GetCommaNode()
        {
            ResetPosition();

            var n = new SequenceNode();
            var buffer = new List<Token>();

            do
            {
                if (ThisToken.Content == ",")
                {
                    n.Sequence.Add(GetAst(buffer));
                    buffer = new List<Token>();
                }
                else
                    buffer.Add(ThisToken);

            } while (NextToken());

            if (buffer.Count > 0)
                n.Sequence.Add(GetAst(buffer));

            return n;
        }
        private void MakeDouble()
        {
            ResetPosition();
            try
            {
                do
                {
                    if ((ThisToken.Content == "." && Position != 0) &&    //  '123', '.', '432' -> '123.432'

                        (this.Expression[Position - 1].Type == type.IntConst &&
                         this.Expression[Position + 1].Type == type.IntConst))
                    {
                        Token double_t = new Token(type.DoubleConst, this.Expression[Position - 1].Content +
                                                                     "." +
                                                                     this.Expression[Position + 1].Content,
                                                                     this.Expression[Position - 1]);

                        this.Expression.RemoveRange(Position - 1, 3);
                        this.Expression.Insert(Position - 1, double_t);
                    }
                }
                while (NextToken());
            }
            catch (Exception) { return; }
        }


        private void MakeBracketPreority()
        {
            var newExpression = new List<Token>();

            ResetPosition();
            do
            {
                if (ThisToken.Content == ")" || ThisToken.Content == "]")
                    Error.ErrMessage(ThisToken, "missing begin bracket");

                if (ThisToken.Content == "(")
                {
                    var bracketExpression = new List<Token>();
                    int count = 0;
                    while (count >= 0 && NextToken())
                    {
                        if (ThisToken.Content == "(")
                        {
                            count++;
                            bracketExpression.Add(ThisToken);
                        }
                        else if (ThisToken.Content == ")")
                        {
                            count--;
                            bracketExpression.Add(ThisToken);
                        }
                        else
                            bracketExpression.Add(ThisToken);
                    }
                    if (count >= 0)
                        Error.ErrMessage(ThisToken, "missing ')'");
                    bracketExpression.RemoveAt(bracketExpression.Count - 1);
                    newExpression.Add(new RoundBracketExpressionToken(bracketExpression));

                }
                else if (ThisToken.Content == "[")
                {
                    var bracketExpression = new List<Token>();
                    int count = 0;
                    while (count >= 0 && NextToken())
                    {
                        if (ThisToken.Content == "[")
                        {
                            count++;
                            bracketExpression.Add(ThisToken);
                        }
                        else if (ThisToken.Content == "]")
                        {
                            count--;
                            bracketExpression.Add(ThisToken);
                        }
                        else
                            bracketExpression.Add(ThisToken);
                    }
                    if (count >= 0)
                        Error.ErrMessage(ThisToken, "missing ']'");
                    bracketExpression.RemoveAt(bracketExpression.Count - 1);
                    newExpression.Add(new SquareBracketExpressionToken(bracketExpression));
                }

                else
                    newExpression.Add(ThisToken);

            } while (NextToken());
            this.Expression = newExpression;
        }

        private bool IsVariableInit() // native public ? a;
        {
            ResetPosition();

            //var modifiers = GetModifiers();

            if (ThisToken.Content == "?")

                if (NextToken())
                
                    if (ThisToken.Type == type.Identifier)
                    {
                        FinishedNode = new VariableInitNode(ThisToken);
                        //FinishedNode.Modifiers = modifiers;
                        return true;
                    }
                    else
                        Error.ErrMessage(ThisToken, "expected name of new variable");
                
                else
                    Error.ErrMessage(ThisToken, "missing variable name");
            
            return false;
        }

        private bool IsConstantInit()
        {
            ResetPosition();

            if (ThisToken.Content == "#")

                if (NextToken())
                
                    if (ThisToken.Type == type.Identifier)
                    {
                        FinishedNode = new ConstantInitNode(ThisToken);
                        //FinishedNode.Modifiers = modifiers;
                        return true;
                    }
                    else
                        Error.ErrMessage(ThisToken, "expected name of new constant");
                
                else
                    Error.ErrMessage(ThisToken, "missing constant name");
            

            return false;
        }

        private bool IsOnlyName()
        {
            if (Expression.Count == 1 && Expression[0].Type == type.Identifier)
            {
                var n = new NameNode(Expression[0]);
                FinishedNode = n;
                return true;
            }
            return false;
        }

        private bool IsOnlyConstant()
        {
            if (Expression.Count == 1
                &&
                (
                Expression[0].Type >= type.BoolConst && Expression[0].Type <= type.StrConst  // all constant types
                ))
            {
                var n = new ConstantNode(Expression[0]);
                FinishedNode = n;
                return true;
            }
            return false;
        }
    }
}
