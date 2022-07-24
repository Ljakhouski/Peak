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
            {
                rightExpr = Constant.GenerateConstInRegister(rightExpr as ConstantResult, st);
            }
            //var leftExpr = Expression.Generate(node.Left, st);

            if (to is IdentifierNode)
            {
                var id = (to as IdentifierNode).Id;
                var identifier = st.GetSymbolFromAllSpaces(id);

                if (identifier == null)
                    Error.ErrMessage(identifier.NameToken, "name does not exist");

                if (identifier is VariableTableElement)
                {
                    if (identifier.Type == rightExpr.ResultType)
                    {
                        return GenerateAssignmentForVariable(id, st, rightExpr.ReturnDataId);
                    }
                        
                }
            }
            throw new CompileException("can only be assigment to variable");
            
        }

        public static GenResult GenerateAssignmentForVariable(Token name, SymbolTable st, MemoryDataId assignmentData)
        {
            var localResult = st.GetFromMethodContext(name) as VariableTableElement;
            if (localResult is null == false)
            {
                st.MemoryAllocator.MoveToRegister(assignmentData);

                var offset = localResult.Id.Rbp_Offset;
                var register = assignmentData.Register.ToString();

                st.Emit(string.Format("mov [rbp {0}], {1}", offset, register));
                /*
                st.MethodCode.Emit(
                    new AsmInstruction()
                    {

                        InstructionName = InstructionName.Mov,

                        FirstOperand = new Operand()
                        {
                            IsGettingAddress = true,
                            RegisterName = RegisterName.RBP,
                            Offset = localResult.Id.StackOffset
                        },

                        SecondOperand = new Operand()
                        {
                            RegisterName = assignmentData.Register
                        }
                    });*/

                assignmentData.Free();
                return new EmptyGenResult();
            }
            else
            {
                return recursiveAccessGenerate(name, st, st, assignmentData, null /* send rbp */);
            }
        }

        private static GenResult recursiveAccessGenerate(Token name, SymbolTable currentMethodScope, SymbolTable st, MemoryDataId assigmentData, MemoryDataId stackAddressingRegister)
        {
            var contextRef = currentMethodScope.GetMethodContextRef();
            var newRefRegisterName = st.MemoryAllocator.GetFreeRegister();
            var newRefRegId = new MemoryDataId(st);
            st.MemoryAllocator.SetIdToFreeRegister(newRefRegId, newRefRegisterName);
            //st.MemoryAllocator.MoveToRegister(contextRef.Id);


            var reg1 = newRefRegisterName.ToString();
            var reg2 = stackAddressingRegister is null? "rbp" : stackAddressingRegister.Register.ToString();
            var offset = contextRef.Id.Rbp_Offset;
            st.Emit(string.Format("mov {0}, [{1} {2}]", reg1, reg2, offset));
            if (stackAddressingRegister is null == false)
                stackAddressingRegister.Free();
            /*
            st.MethodCode.Emit(new AsmInstruction()
            {
                InstructionName = InstructionName.Mov,
                FirstOperand = new Operand()
                {
                    RegisterName = newRefRegister
                },
                SecondOperand = new Operand()
                {
                    IsGettingAddress = true,
                    RegisterName = stackAddressingRegister,
                    Offset =contextRef.Id.Rbp_Offset
                }
            });*/

            var variable = contextRef.Context.GetFromMethodContext(name) as VariableTableElement;

            if (variable is null)
            {
                if (currentMethodScope is GlobalSymbolTable)
                    Error.ErrMessage(name, "name does not exist");
                else
                    return recursiveAccessGenerate(name, contextRef.Context, st, assigmentData, newRefRegId);
            }    
            else
            {
                st.MemoryAllocator.MoveToRegister(variable.Id);

                var reg1_ = newRefRegisterName.ToString();
                var offset_ = variable.Id.Rbp_Offset;
                var reg2_ = variable.Id.Register.ToString();

                st.Emit(string.Format("mov [{0} {1}], {2}", reg1, offset, reg2));
                /*st.MethodCode.Emit(new AsmInstruction()
                {
                    InstructionName = InstructionName.Mov,
                   
                    FirstOperand = new Operand()
                    {
                        IsGettingAddress = true,
                        RegisterName = newRefRegister,
                        Offset = variable.Id.Rbp_Offset
                    },

                    SecondOperand = new Operand()
                    {
                        RegisterName = variable.Id.Register
                    }
                });*/

                return new EmptyGenResult();
            }
            throw new CompileException();
        }
    }
}
