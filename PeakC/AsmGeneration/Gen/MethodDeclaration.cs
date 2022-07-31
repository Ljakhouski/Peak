using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration.Gen
{
    static class MethodDeclaration
    {
        public static GenResult Generate(MethodNode node, SymbolTable st)
        {
            var type = new SemanticType(node);
            var e = getMethod(node.Name, type, st);
            if (e is null == false)
                Error.ErrMessage(node.Name, "method already exist in current scope");

            var tableElement = new MethodTableElement()
            {
                NameToken = node.Name,
                Convention = CallConvention.x64_win,
                IsDllImportMethod = node.IsFromDll(),
                MethodSignature = type,

            };

            if (node.IsFromDll())
            {
                if (e.NameToken == node.Name)
                    Error.ErrMessage(node.Name, "dll-import methods not supported overloading");
                st.RegisterMethod(tableElement);

                st.MainAssembly.AddDllImportSymbol(node.DllPath.Content, tableElement.Name);

                return new EmptyGenResult();
            }
            else
                throw new CompileException("only dll import methods!");
        }

        private static MethodTableElement getMethod(Token name, SemanticType type, SymbolTable st)
        {
            foreach (var e in st.Data)
                if (e is MethodTableElement && (e as MethodTableElement).MethodSignature == type && e.NameToken == name)
                    return e as MethodTableElement;
                else if (e is MethodContextReferenceElement)
                {
                    var e_ = getMethod(name, type, (e as MethodContextReferenceElement).Context);

                    if (e_ is null)
                        continue;
                    else
                        return e_;
                }
            if (st is MethodSymbolTable == false)
                return getMethod(name, type, st.Prev);
            else
                return null;
                
            // TODO: search in static global context
           
        }

    }
}
