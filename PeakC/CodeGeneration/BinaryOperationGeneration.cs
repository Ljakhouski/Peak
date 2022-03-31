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

            
            var right = generateByteCode(n.Right, currentSymbolTable);
            var left = generateStoreData(n.Left, currentSymbolTable);

            if (left.ExprResult.Equals(right.ExprResult))
            {
                return new GenerationResult() { Nothing = true };
            }
            else
                Error.ErrMessage(n.Operator,
                    "assignment " + left.ExprResult.ToString() 
                    + " and " + right.ExprResult.ToString()
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
                (left.ExprResult.Value == SymbolType.Type.Int
             || left.ExprResult.Value == SymbolType.Type.Double)
             &&
             (  right.ExprResult.Value == SymbolType.Type.Int
             || right.ExprResult.Value == SymbolType.Type.Double))
            {

                var result = new GenerationResult() { Nothing = false, ExprResult = left.ExprResult };
                
                result.GeneratedByteCode.AddByteCode(left);
                result.GeneratedByteCode.AddByteCode(right);

                switch (n.Operator.Content)
                {
                    case "+":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Add);
                        break;
                    case "-":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Sub);
                        break;
                    case "*":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Mul);
                        break;
                    case "/":
                        result.GeneratedByteCode.AddByteCode(InstructionName.Div);
                        break;
                }

                return result;
            }
            else if (left.ExprResult == right.ExprResult &&
               left.ExprResult.Value == SymbolType.Type.Str)
            {
                throw new Exception("temporarily unsupported");
            }
            else
                Error.ErrMessage(n.Operator,
                    "operator \"" + n.Operator + "\" does not accept type " + left.ExprResult.ToString()
                    + " and " + right.ExprResult.ToString());
            throw new Exception();
        }
    }
}
