using System;
using System.Collections.Generic;
using System.Text;
using Peak.PeakC;
using Peak.PeakC.Parser;
using RuntimeEnvironment;
using RuntimeEnvironment.RuntimeModule;

namespace Peak.CodeGeneration
{
    partial class ByteCodeGenerator
    {
        private GenerationResult generateForAssignment(BinaryNode n, SymbolTable currentSymbolTable)
        {
            if (n.Operator != "<<")
                throw new Exception();

            var left = generateByteCode(n.Left, currentSymbolTable);
            var right = generateByteCode(n.Right, currentSymbolTable);


            if (left.Result.Equals(right.Result))
            {
                var method = currentModule.Methods[currentSymbolTable.GeneratedMethodAddress];
                addByteCode(method, CommandName.Set);
                return new GenerationResult() { Nothing = true };
            }
            else
                Error.ErrMessage(n.Operator,
                    "assignment " + left.Result.ToString() 
                    + " and " + right.Result.ToString()
                    + " not possible" );
            throw new Exception();
        }

        private GenerationResult generateForMathOperators(BinaryNode n, SymbolTable currentSymbolTable)
        {
            var left = generateByteCode(n.Left, currentSymbolTable);
            var right = generateByteCode(n.Right, currentSymbolTable);

            if (left.Nothing)
                Error.ErrMessage(n.Left.MetaInf, "expression must be return \"int\" or \"double\"");
            
            else if (right.Nothing)
                Error.ErrMessage(n.Right.MetaInf, "expression must be return \"int\" or \"double\"");
            

            if (/*left.Result == right.Result &&*/
                (left.Result.Value == SymbolType.Type.Int
             || left.Result.Value == SymbolType.Type.Double)
             &&
             (  right.Result.Value == SymbolType.Type.Int
             || right.Result.Value == SymbolType.Type.Double))
            {
                var method = currentModule.Methods[currentSymbolTable.GeneratedMethodAddress];

                switch (n.Operator.Content)
                {
                    case "+":
                        addByteCode(method, CommandName.Add);
                        break;
                    case "-":
                        addByteCode(method, CommandName.Sub);
                        break;
                    case "*":
                        addByteCode(method, CommandName.Mul);
                        break;
                    case "/":
                        addByteCode(method, CommandName.Div);
                        break;
                }
                
                return new GenerationResult() { Nothing = false, Result = left.Result};
            }
            else if (left.Result == right.Result &&
               left.Result.Value == SymbolType.Type.Str)
            {
                throw new Exception("temporarily unsupported");
            }
            else
                Error.ErrMessage(n.Operator,
                    "operator \"" + n.Operator + "\" does not accept type " + left.Result.ToString()
                    + " and " + right.Result.ToString());
            throw new Exception();
        }
    }
}
