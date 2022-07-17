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
                        return GenerateForVariableAccess(id, st, rightExpr.ReturnDataId);
                    }
                        
                }
            }
            throw new CompileException();
            
        }

        public static GenResult GenerateForVariableAccess(Token name, SymbolTable st, MemoryDataId assignmentData)
        {
            var localResult = st.GetFromMethodContext(name) as VariableTableElement;
            if (localResult is null == false)
            {
                st.MemoryAllocator.MoveToRegister(assignmentData);

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
                    });

                return new EmptyGenResult();
            }
            else
            {
                return recursiveAccessGenerate(name, st, st, assignmentData, RegisterName.RBP);
            }
        }

        private static GenResult recursiveAccessGenerate(Token name, SymbolTable currentMethodScope, SymbolTable st, MemoryDataId assigmentData, RegisterName stackAddressingRegister)
        {
            var contextRef = currentMethodScope.GetMethodContextRef();
            var newRefRegister = st.MemoryAllocator.GetFreeRegister();
            st.MemoryAllocator.MoveToRegister(contextRef.Id);

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
                    Offset = st.MemoryAllocator.CalculateLocalOffset(contextRef)
                }
            });

            var variable = contextRef.Context.GetFromMethodContext(name) as VariableTableElement;

            if (variable is null)
            {
                if (currentMethodScope is GlobalSymbolTable)
                    Error.ErrMessage(name, "name does not exist");
                else
                    return recursiveAccessGenerate(name, contextRef.Context, st, assigmentData, newRefRegister);
            }    
            else
            {
                st.MemoryAllocator.MoveToRegister(variable.Id);

                st.MethodCode.Emit(new AsmInstruction()
                {
                    InstructionName = InstructionName.Mov,
                   
                    FirstOperand = new Operand()
                    {
                        IsGettingAddress = true,
                        RegisterName = newRefRegister,
                        Offset = st.MemoryAllocator.CalculateLocalOffset(variable)
                    },

                    SecondOperand = new Operand()
                    {
                        RegisterName = variable.Id.Register
                    }
                });

                return new EmptyGenResult();
            }
            throw new CompileException();
        }
    }
}
