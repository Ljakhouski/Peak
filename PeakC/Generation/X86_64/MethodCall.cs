using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
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

            var method = getMethodAddress(node, calledSignature, methodSt);

            //insert ref on frame as first argument
            var fullSignature = method.fullSignature as MethodSemanticType;
            if (fullSignature.MethodContext is null == false) 
            {
                argsResult.Insert(0, getContextRef(st, fullSignature.MethodContext));
            }

            if (method.IsDllMethod)
                method.Label = $"[{method.Label}]";
            call_x86_64(argsResult.ToArray(), st, label: method.Label, methodObj: method.DynamicResult.ReturnDataId);

            if (fullSignature.RetType == null)
                return new EmptyGenResult();
            else
                return new GenResult()
                {
                    ResultType = (method.fullSignature as MethodSemanticType).RetType,
                    ReturnDataId = MemoryIdTracker.FuncResult(st)
                };
        }

        private static GenResult getContextRef(SymbolTable st, MethodContextReferenceType needContext)
        {
            if (st.MethodTable.Id == needContext.Context.Id)
            {/*  TODO: make this variant
                return new GenResult()
                {
                    ResultType = needContext,
                    ReturnDataId = new MemoryIdTracker(st, 8) {IsRbp = true }
                };*/
                var reg = st.MemoryAllocator.GetFreeRegister();
                st.Emit($"mov {reg}, rbp");
                var id = new MemoryIdTracker(st, size: 8);
                st.MemoryAllocator.SetRegister(id, reg);
                return new GenResult()
                {
                    ResultType = needContext,
                    ReturnDataId = id
                };
            }
            else
            {
                var mref = st.GetMethodContextRef();
                MemoryIdTracker mRefTracker = null;
                if (mref.Id == needContext.Context.Id)
                {
                    mRefTracker = mref.MemoryId;
                }
                else
                {
                    while (mref.Context.MethodTable is null == false)
                    {
                        var nMfref = mref.Context.GetMethodContextRef();
                        if (nMfref is null)
                            break;
                        else
                        {
                            if (nMfref.Context.GetMethodContextRef().Id == needContext.Context.Id)
                                mRefTracker = nMfref.MemoryId;
                            else
                                continue;
                        }
                    }

                    if (mRefTracker is null)
                        Error.ErrMessage("reference on this method not found");

                    //st.MemoryAllocator.MoveToAnyRegister(mRefTracker);

                }

                return new GenResult()
                {
                    ResultType = needContext,
                    ReturnDataId = mRefTracker
                };

            }
        }

        public static GenResult Generate(GenResult methodObject, MethodCallNode node, SymbolTable st)
        {
            throw new CompileException("method-object not implemented");
        }
        private static void call_x86_64(GenResult[] args, SymbolTable st, string label = "", MemoryIdTracker methodObj = null)
        {
            int N = args.Length;
            saveRegisters(st, expect: args);
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
                    st.MemoryAllocator.MoveToRegister(args[0], RegisterName.xmm0);
                else
                    st.MemoryAllocator.MoveToRegister(args[0], RegisterName.rcx);

                block(RegisterName.xmm0, RegisterName.rcx);
                // TODO: make for 128 bit SSE registers
            }
            if (N >= 2)
            {
                if (args[1].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[1], RegisterName.xmm1);
                else
                    st.MemoryAllocator.MoveToRegister(args[1], RegisterName.rdx);

                block(RegisterName.xmm1, RegisterName.rdx);
            }
            if (N >= 3)
            {
                if (args[2].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[2], RegisterName.xmm2);
                else
                    st.MemoryAllocator.MoveToRegister(args[2], RegisterName.r8);

                block(RegisterName.xmm2, RegisterName.r8);
            }
            if (N >= 4)
            {
                if (args[0].IsSSE_Data())
                    st.MemoryAllocator.MoveToRegister(args[0], RegisterName.xmm3);
                else
                    st.MemoryAllocator.MoveToRegister(args[0], RegisterName.r9);

                block(RegisterName.xmm3, RegisterName.r9);
            }

            if (methodObj is null == false)
            {
                st.MemoryAllocator.MoveToAnyRegister(methodObj);
                st.Emit($"call [{methodObj.Register}]");
            }
            else
            {
                st.Emit($"call {label}");
            }

            unblock(RegisterName.xmm0, RegisterName.rcx);
            unblock(RegisterName.xmm1, RegisterName.rdx);
            unblock(RegisterName.xmm2, RegisterName.r8);
            unblock(RegisterName.xmm3, RegisterName.r9);

            restoreStackAlign(N, st);

            return;

            void block(RegisterName r1, RegisterName r2)
            {
                st.MemoryAllocator.Block(r1);
                st.MemoryAllocator.Block(r2);
            }

            void unblock(RegisterName r1, RegisterName r2)
            {
                st.MemoryAllocator.Unblock(r1);
                st.MemoryAllocator.Unblock(r2);
            }
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
        private static void saveRegisters(SymbolTable table, GenResult[] expect)
        {
            var blockRegisters = new List<RegisterName>();
            foreach (var e in expect)
            {
                if (e is ConstantResult)
                    continue;
                else
                {
                    if (e.ReturnDataId.ExistInRegisters || e.ReturnDataId.ExistInSSERegisters)
                        blockRegisters.Add(e.ReturnDataId.Register);
                }
            }


            foreach(var e in table.MemoryAllocator.RegisterMap)
            {
                //if (e.Register == RegisterName.rax)
                //    continue;
                /*else*/
                if (e.ContainedData is null == false &&
                    e.ContainedData.ExistInStack == false &&
                    blockRegisters.Contains(e.ContainedData.Register) == false)
                {
                    table.MemoryAllocator.MoveToStack(e.ContainedData);
                    e.Free();
                }
            }
            foreach (var e in table.MemoryAllocator.SSERegisterMap)
            {
                if (e.ContainedData is null == false &&
                    e.ContainedData.ExistInStack == false &&
                    blockRegisters.Contains(e.ContainedData.Register) == false)
                {
                    table.MemoryAllocator.MoveToStack(e.ContainedData);
                    e.Free();
                }
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

        }

        private class MethodSearchResult
        {
            public GenResult DynamicResult { get; set; }
            public string Label { get; set; } = null;
            public bool IsDllMethod = false;
            public SemanticType fullSignature { get; set; } // with return type
        }
        private static MethodSearchResult getMethodAddress(MethodCallNode node, SemanticType signature, SymbolTable st/*, SymbolTable context*/)
        {
            if (node.From is IdentifierNode)
            {
                var method = st.GetVisibleMethodTableElement((node.From as IdentifierNode).Id, signature);
                if (method is null)
                    Error.ErrMessage((node.From as IdentifierNode).Id, "method not exist");


                return new MethodSearchResult()
                {
                    Label = method.Label,
                    fullSignature = method.MethodSignature,
                    DynamicResult = new ConstantResult()
                    {
                        ConstValue = (node.From as IdentifierNode).Id, // call label
                        ResultType = new SemanticType(Type.Str)
                    },
                    IsDllMethod = method.IsDllImportMethod
                };
            }
            else
            {
                var id = Expression.Generate(node.From, st);
                if (id.ResultType == signature)
                    return new MethodSearchResult() { DynamicResult = id };
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
