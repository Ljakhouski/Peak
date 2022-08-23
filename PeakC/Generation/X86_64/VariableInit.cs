using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class VariableInit
    {
        public static GenResult Generate(VariableInitNode node, SymbolTable st)
        {
            if (st.ContainsInAllSpaces(node.Name))
            {
                Error.ErrMessage(node.Name, "name already exist");
            }
            else
            {
                if (node.Type == null)  // #varName <- <expression>; 
                {
                    var rightExpr = Expression.Generate(node.RightExpression, st);

                    if (rightExpr is ConstantResult)
                        rightExpr = (rightExpr as ConstantResult).MoveToRegister(st);
                    

                    var rightExprDataId = rightExpr.ReturnDataId;

                    if (rightExpr is EmptyGenResult)
                        Error.ErrMessage(node.Name, "expression hasn't type");
                    else
                    {
                        var e = new VariableTableElement(st, node.Name, rightExpr.ResultType);

                        st.RegisterVariable(e);                                       
                        st.MemoryAllocator.MoveToStack(rightExprDataId, e.IdTracker);
                        st.MemoryAllocator.FreeFromRegister(rightExprDataId);
                        //Assignment.Generate(node.Name, st, rightExprDataId);
                        //st.MemoryAllocator.MoveRegisterToStack(rightExprDataId, e.Id, st);
                        /*
                        st.MethodCode.Emit(InstructionName.Mov, 
                        new Operand() 
                        { 
                            IsGettingAddress = true, 
                            RegisterName = rightExprDataId.Register,
                            Offset = st.MemoryAllocator.CalculateLocalOffset(e.Id.StackOffset, st)
                        },
                        rightExprDataId.Register);*/

                        return new EmptyGenResult();
                    }
                }
                else  // int varName <- <expression>;  ||  int varName;
                {
                    var declaredType = new SemanticType(node.Type);

                    if (node.RightExpression != null)
                    {
                        var rightExpr = Expression.Generate(node.RightExpression, st);

                        if (declaredType == rightExpr.ResultType)
                        {
                            var e = new VariableTableElement(st, node.Name, declaredType);

                            st.RegisterVariable(e);
                            var assigmentData = st.MemoryAllocator.MoveToAnyRegister(rightExpr);
                            st.MemoryAllocator.MoveToStack(assigmentData, e.IdTracker);
                            st.MemoryAllocator.FreeFromRegister(assigmentData);
                            return new EmptyGenResult();
                        }
                        else
                            Error.ErrMessage(node.Name, "types does not match");
                    }
                    else
                    {
                        var e = new VariableTableElement(st, node.Name, declaredType);

                        st.RegisterVariable(e);

                        return new EmptyGenResult();
                    }
                }
            }
            return new EmptyGenResult();
        }
    }
}
