using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.AsmGeneration
{
    static class MethodCall
    {
        /* https://docs.microsoft.com/en-us/cpp/build/x64-calling-convention?view=msvc-170 */
        public static GenResult Generate(MethodCallNode node, SymbolTable st, SymbolTable methodSt)
        {
            var args = getArgsInArray(node.Args);
            var argsResult = new List<GenResult>();

            foreach (var arg in args)
                argsResult.Add(Expression.Generate(arg, st));

            var calledSignature = new MethodSemanticType()
            {
                IsNothing = false,
                Type = Type.Method,
                RetType = new SemanticType() { Type = Type.AnyToCompare },
                Args =  convertToType(argsResult)

            };

            var method = getMethod(node, calledSignature, methodSt);

            /*if (method is null)
                Error.ErrMessage(node.MetaInf, "method not exist in current scope");*/

            /* if (method is VariableTableElement)
             {
                 Identifier.Generate(method.Id, st);
             }*/



            // TODO: get context reference
            /*if (method.ExternContextRef is null == false)
            {

            }*/

            if (method is ConstantResult)
                call_x86_64(argsResult.ToArray(), st, label: (method as ConstantResult).ConstValue.Content);
            else
                call_x86_64(argsResult.ToArray(), st, methodObj: method.ReturnDataId);
            return new GenResult()
            {
                ResultType = calledSignature,
                ReturnDataId = MemoryDataId.FuncResult(st)
            };
        }
        public static GenResult Generate(GenResult methodObject, MethodCallNode node, SymbolTable st)
        {
            throw new CompileException("method-object not implemented");
        }
        private static void call_x86_64(GenResult[] args, SymbolTable st, string label = "", MemoryDataId methodObj = null)
        {
            int N = args.Length;

            alignStackBeforePush(N, st);
            if (N>=5)
            {                
                for (int i = args.Length; i>4; i--)
                {
                    args[i].PushOnStack(st);
                }
            }

            if (N >= 1)
            {
                if (args[0].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[0].ReturnDataId, RegisterName.xmm0);
                else
                    st.MemoryAllocator.MoveToRegister(args[0].ReturnDataId, RegisterName.rcx);

                // TODO: make for 128 bit SSE registers
            }
            if (N >= 2)
            {
                if (args[1].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[1].ReturnDataId, RegisterName.xmm1);
                else
                    st.MemoryAllocator.MoveToRegister(args[1].ReturnDataId, RegisterName.rdx);
            }
            if (N >= 3)
            {
                if (args[2].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[2].ReturnDataId, RegisterName.xmm2);
                else
                    st.MemoryAllocator.MoveToRegister(args[2].ReturnDataId, RegisterName.r8);
            }
            if (N >= 4)
            {
                if (args[0].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[0].ReturnDataId, RegisterName.xmm3);
                else
                    st.MemoryAllocator.MoveToRegister(args[0].ReturnDataId, RegisterName.r9);
            }



            restoreStackAlign(N, st);
        }

        private static void alignStackBeforePush(int n, SymbolTable st)
        {
            if (n <= 4)
                return;
            if ((n-4)%2 != 0)
            {
                st.Emit("sub rsp, 8");
            }
        }
        private static void restoreStackAlign(int n, SymbolTable st)
        {
            if (n <= 4)
                return;
            if ((n - 4) % 2 != 0)
            {
                st.Emit("add rsp, 8");
            }
        }
        private static void pushArg(GenResult r, SymbolTable st)  // ?
        {
            if (r is ConstantResult)
            {
                (r as ConstantResult).PushOnStack(st);
            }
            if (r.ReturnDataId.IsSSE_Element)
            {
                throw new CompileException("SSE not implemented");
            }
            else
            {
                var arg = r.ReturnDataId;
                st.MemoryAllocator.MoveToAnyRegister(arg);

                st.Emit($"push {arg.Register}");
            }
            
        }

        private static List<SemanticType> convertToType(List<GenResult> args)
        {
            if (args.Count == 0)
                return null;
            else
            {
                var l = new List<SemanticType>();
                foreach (var arg in args)
                {
                    l.Add(arg.ResultType);
                }
                return l;
            }
                

        }/*
        [Obsolete]
        private static TableElement getStaticMethodObsolete(MethodCallNode node, SemanticType signature, SymbolTable st)
        {
            foreach(var e in st.Data)
            {
                if (e is MethodTableElement)
                {
                    if ((e as MethodTableElement).MethodSignature == signature)
                    {
                        return e as MethodTableElement;
                    }    
                }
                else if (e is VariableTableElement)
                {
                    if ((e as VariableTableElement).Type == signature)
                        return e as VariableTableElement;
                }
                else if (e is MethodContextReferenceElement)
                {
                    var m = getMethod(node, signature, (e as MethodContextReferenceElement).Context);
                    if (m is null)
                        continue;
                    else
                        return m;
                }
            }

            if (st.Prev != null)
                return getMethod(node, signature, st.Prev);
            return null;
        }*/
        private static GenResult getMethod(MethodCallNode node, SemanticType signature, SymbolTable st/*, SymbolTable context*/)
        {
            if (node.From is IdentifierNode)
            {
                var method = st.GetVisibleMethodTableElement((node.From as IdentifierNode).Id);
                if (method is null)
                    Error.ErrMessage((node.From as IdentifierNode).Id, "method not exist");

                return new ConstantResult()
                {
                    ConstValue = (node.From as IdentifierNode).Id,
                    ResultType = new SemanticType(Type.Str)
                };
            }
            else
            {
                var id = Expression.Generate(node.From, st);
                if (id.ResultType == signature)
                    return id;
                else
                    Error.ErrMessage(node.MetaInf, "method not exist");
            }
            throw new CompileException();
        }

        private static Node[] getArgsInArray(Node n)
        {

            if (n is SequenceNode)
            {
                var arr = new List<Node>();
                foreach (var e in (n as SequenceNode).Sequence)
                    arr.Add(e);
                return arr.ToArray();
            }
            else
                return new Node[1] { n };
        }
    }
}
