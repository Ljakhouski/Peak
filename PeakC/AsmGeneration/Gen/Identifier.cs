using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class Identifier
    {
        /*  generate code to get access to the identifier (variable/const/method-oject/...) 
         *  data can be contains in the stack, then need to generate code to write 
         *  variable-data to the register or return information about variable-position
         *  (only for last method-context on the top-frame. 
         *  For using this data leter, need to move variable from stack to register ) 
         */

        public static GenResult Generate(IdentifierNode node, SymbolTable st)
        {
            var data = st.GetSymbolFromVisibleSpaces(node.Id);
            var id = data.Id;

            if (data is ConstTableElement)
            {
                return new ConstantResult()
                {
                    ResultType = data.Type,
                    //IntValue = int.(data as ConstTableElement).ConstValue.Value,\
                    ConstValue = (data as ConstTableElement).ConstValue
                };
            }
            else if (data is VariableTableElement)
            {
                return SymbolTableSearching.GenerateGettingData(data, st, st); 
            }
            else if (data is MethodTableElement) 
            {

                Error.ErrMessage("method as variable not supported");
                var methodData = st.GetVisibleMethodTableElement(node.Id);
                if (methodData is null)
                    Error.ErrMessage(node.Id, "name does not exist");
                else
                    return new ConstantResult()
                    {
                        ResultType = new SemanticType(Type.Str),
                        ConstValue = methodData.NameToken
                    };
            }
            else if (data is null)
                Error.ErrMessage(node.MetaInf, "name does not exist");
            else
                throw new CompileException();
            throw new CompileException();
        }

      /*  public static GenResult Generate(IdentifierNode node, SymbolTable st, GenResult specialContext)
        {
            StructSymbolTable structContrext;
            if (specialContext.ResultType.Type == Type.Struct)
            
            var data = specialContext.GetSymbolFromVisibleSpaces(node.Id);
            var id = data.Id;

            if (data is ConstTableElement)
            {
                return new ConstantResult()
                {
                    ResultType = data.Type,
                    //IntValue = int.(data as ConstTableElement).ConstValue.Value,\
                    ConstValue = (data as ConstTableElement).ConstValue
                };
            }
            else if (data is VariableTableElement)
            {
                return SymbolTableSearching.GenerateGettingData(data, st, st);
            }
            else if (data is MethodTableElement)
            {

                Error.ErrMessage("method as variable not supported");
                var methodData = st.GetVisibleMethodTableElement(node.Id);
                if (methodData is null)
                    Error.ErrMessage(node.Id, "name does not exist");
                else
                    return new ConstantResult()
                    {
                        ResultType = new SemanticType(Type.Str),
                        ConstValue = methodData.NameToken
                    };
            }
            else if (data is null)
                Error.ErrMessage(node.MetaInf, "name does not exist");
            else
                throw new CompileException();
            throw new CompileException();
        }*/
    }
}
