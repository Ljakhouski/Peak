using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class Assignment
    {
        public static GenResult Generate(Node from, Node to, SymbolTable st)
        {
            var rightExpr = Expression.Generate(from, st);

            if (rightExpr is ConstantResult)
                (rightExpr as ConstantResult).MoveToRegister(st);

            //var leftExpr = Expression.Generate(node.Left, st);

            if (to is IdentifierNode)
            {
                return generateForVarInCodeBlock(from, to as IdentifierNode, st);
            }
            throw new CompileException("can only be assigment to variable");
            
        }

        private static GenResult generateForVarInCodeBlock(Node from, IdentifierNode to, SymbolTable st)
        {
            var id = (to as IdentifierNode).Id;
            var identifier = st.GetSymbolFromVisibleSpaces(id);

            var data = Expression.Generate(from, st);

            if (identifier == null)
                Error.ErrMessage(identifier.NameToken, "name does not exist");

            if (identifier is VariableTableElement)
            {
                if (identifier.Type == data.ResultType)
                {
                    SymbolTableSearching.GenerateSettingData(identifier, data.ReturnDataId, st, st);// GenerateAssignmentForVariable(id, st, rightExpr.ReturnDataId);
                }

            }
            else
                throw new CompileException();

            return new EmptyGenResult();
        }
    }
}
