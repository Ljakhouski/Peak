using Peak.PeakC;
using Peak.AsmGeneration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
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
                    var type = Expression.Generate(node.RightExpression, st);

                    if (type is ConstantResult)
                        (type as ConstantResult).MoveToRegister(st);
                    

                    var rightExprDataId = type.ReturnDataId;

                    if (type is EmptyGenResult)
                        Error.ErrMessage(node.Name, "expression hasn't type");
                    else
                    {
                        var e = new VariableTableElement(st, node.Name, type.ResultType);

                        st.RegisterVariable(e);

                       
                       
                        st.MemoryAllocator.MoveToRegister(rightExprDataId);

                        SymbolTableSearching.GenerateSettingData(e, rightExprDataId, st, st);
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
                            st.MemoryAllocator.MoveToRegister(rightExpr.ReturnDataId);
                            SymbolTableSearching.GenerateSettingData(e, rightExpr.ReturnDataId, st, st);
                            //Assignment.GenerateAssignmentForVariable(node.Name, st, rightExpr.ReturnDataId);
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
