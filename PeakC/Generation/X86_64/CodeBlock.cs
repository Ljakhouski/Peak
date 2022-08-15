using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class CodeBlock
    {
        public static GenResult Generate(CodeBlockNode node, SymbolTable st)
        {
            var result = new EmptyGenResult() { ResultType = new SemanticType() };

            foreach (Node n in node.Node)
            {
                if (n is VariableInitNode)
                {
                    VariableInit.Generate(n as VariableInitNode, st);
                }
                else if (n is BinaryNode)
                {
                    switch ((n as BinaryNode).Operator.Content)
                    {
                        case "<-":
                            Expression.Generate(n, st);
                            break;
                        //case "":
                        default:
                            throw new CompileException();
                            break;
                    }
                }
                else if (n is MethodNode)
                {
                    MethodDeclaration.Generate(n as MethodNode, st);
                }
                else if (n is MethodCallNode)
                {
                    MethodCall.Generate(n as MethodCallNode, st, st);
                }
                else if (n is WordOperatorNode)
                {
                    switch ((n as WordOperatorNode).Operator)
                    {
                        case "return":
                            ControlStatement.GenerateReturn(n as WordOperatorNode, st);
                            break;
                        default:
                            throw new CompileException();
                    }
                }
                else
                    throw new CompileException();

            }

            return result;
        }
    }
}
