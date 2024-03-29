﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class Assignment
    {
        public static GenResult Generate(Node from, Node to, SymbolTable st)
        {
            var data = Expression.Generate(from, st);

            if (data is ConstantResult)
                (data as ConstantResult).MoveToRegister(st);

            //var leftExpr = Expression.Generate(node.Left, st);

            if (to is IdentifierNode)
            {
                return generateForVarInCodeBlock(data, to as IdentifierNode, st);
            }
            throw new CompileException("can only be assigment to variable");
            
        }

        private static GenResult generateForVarInCodeBlock(GenResult data, IdentifierNode to, SymbolTable st)
        {
            var id = (to as IdentifierNode).Id;
            var identifier = st.GetSymbolFromVisibleSpaces(id);


            if (identifier == null)
                Error.ErrMessage(identifier.NameToken, "name does not exist");

            if (identifier is VariableTableElement)
            {
                if (identifier.Type == data.ResultType)
                {
                    st.MemoryAllocator.MoveToStack(data.ReturnDataId, (identifier as VariableTableElement).IdTracker);
                }

            }
            else
                throw new CompileException();

            return new EmptyGenResult();
        }
    }
}
