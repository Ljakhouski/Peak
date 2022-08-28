using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation.X86_64
{
    static class MethodDeclaration
    {
        public static GenResult Generate(MethodNode node, SymbolTable st)
        {
            var type = new MethodSemanticType(node);

            
            //type.Args.Insert(0, new SemanticType() { Type = Type.ContextRef });

            var e = getMethod(node.Name, type, st);
            if (e is null == false)
                Error.ErrMessage(node.Name, "method already exist in current scope");

            var tableElement = new MethodTableElement()
            {
                NameToken = node.Name,
                Label = $"{node.Name.Content}_{IdGenerator.GenerateMethodId()}",
                Convention = CallConvention.x64_win,
                IsDllImportMethod = node.IsFromDll(),
                MethodSignature = type,
                
            };

            if (node.IsFromDll())
            {
                tableElement.Label = node.Name.Content;
                if (st.GetSymbolFromVisibleSpaces(node.Name) is null == false)
                    Error.ErrMessage(node.Name, "dll-import methods not supported overloading");
                st.RegisterMethod(tableElement);

                st.MainAssembly.AddDllImportSymbol(node.DllPath.Content, tableElement.Name);

                return new EmptyGenResult();
            }
            else
            {
                var mst = new MethodSymbolTable()
                {
                    Prev = st,
                    Signature = type
                };


                

                var actualyArgs = new List<TableElement>();
                var actualyArgsTrackers = new List<MemoryIdTracker>();

                if (isNestedMethod(mst))
                {
                    var mMefType = new MethodContextReferenceType()
                    {
                        CompareContext = st.MethodTable,
                        Type = Type.ContextRef,
                    };

                    type.MethodContext = mMefType;
                    // st.MemoryAllocator.SetRegister(memId, RegisterName.rcx);
                    var mRefElement = new MethodContextReferenceElement(mst)
                    {
                        Context = st,
                        Type = mMefType
                    };

                    mst.RegisterContextRef(mRefElement);                    
                    actualyArgs.Add(mRefElement);
                    actualyArgsTrackers.Add(mRefElement.IdTracker);
                }

                
                var args = getArgs(node);

                for(int i = 0; i< args.Count; i++) 
                {
                    var t = new VariableIdTracker(mst, size: 8);
                    var v = new VariableTableElement(t, args[i].Name, new SemanticType(args[i].Type));
                    
                    mst.RegisterVariable(v);
                    
                    actualyArgsTrackers.Add(t);
                    actualyArgs.Add(v);
                }

               
                placeId_x86_64(mst.MemoryAllocator, actualyArgs, actualyArgsTrackers);
                


                var address = new MemoryIdTracker(mst, size: 8);
                var addressArea = mst.MemoryAllocator.AllocateAreaInStack(address, alignment: 8);
                addressArea.ContainedData = address;

                var rbp = new MemoryIdTracker(mst, size: 8);
                var rbpArea = mst.MemoryAllocator.AllocateAreaInStack(rbp, alignment: 8);
                rbpArea.ContainedData = rbp;

                mst.MemoryAllocator.RBP_dataId = rbp;

                mst.MethodCode.MethodName = tableElement.Label;

                st.RegisterMethod(tableElement);

                emitPrologue(mst, tableElement);
                CodeBlock.Generate(node.Code, mst);
                EmitEpilogue(mst);

                st.MainAssembly.Code.Add(mst.MethodCode);
                return new EmptyGenResult();
            }            
        }

        private static void placeId_x86_64(MemoryAllocator alloc, List<TableElement> actualyArgs, List<MemoryIdTracker> actualyArgsTrackers)
        {
            for (int i = 0; i < actualyArgs.Count; i++)
            {
                var t = actualyArgsTrackers[i];

                bool isSSE = t.IsSSE_Element;
                switch (i)
                {
                    case 0:
                        alloc.SetRegister(t, isSSE ? RegisterName.xmm0 : RegisterName.rcx);
                        break;
                    case 1:
                        alloc.SetRegister(t, isSSE ? RegisterName.xmm1 : RegisterName.rdx);
                        break;
                    case 2:
                        alloc.SetRegister(t, isSSE ? RegisterName.xmm2 : RegisterName.r8);
                        break;
                    case 3:
                        alloc.SetRegister(t, isSSE ? RegisterName.xmm3 : RegisterName.r9);
                        break;
                    default:
                        goto allocInStack;
                }
            }

            allocInStack:

            for (int i = actualyArgs.Count - 1; i>=0; i--)
            {
                var area = alloc.AllocateAreaInStack(actualyArgsTrackers[i], alignment: 8);
                area.ContainedData = actualyArgsTrackers[i];
            }
        }

        private static void emitPrologue(SymbolTable st, MethodTableElement e)
        {

           // st.Emit($"{e.Label}:");
            st.Emit("push rbp");
            st.Emit("mov rbp, rsp");
            st.Emit("sub rsp, ...?");
        }
        public static void EmitEpilogue(SymbolTable st)
        {
            int size = st.MemoryAllocator.GetFrameSize();
            size = MemoryAllocator.AlignUpAbsolute(size, 16);
            st.Emit($"add rsp, { size }");
            st.Emit("pop rbp");
            st.Emit("ret");

            // fix prologue:

            st.MethodCode.Code[2].content = $"sub rsp, {size}";
        }
        private static bool isNestedMethod(SymbolTable st)
        {
            var mt = st.MethodTable;
            if (mt.Prev != null)
                return true;
            return false;
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

        private static List<VariableInitNode> getArgs(MethodNode node)
        {
            var l = new List<VariableInitNode>();

            if (node.Args is SequenceNode)
            {
                foreach (var e in (node.Args as SequenceNode).Sequence)
                    if (e is VariableInitNode)
                        l.Add(e as VariableInitNode);
                    else
                        Error.ErrMessage(e.MetaInf, "variable inizialization expected");
            }
            else if (node.Args is VariableInitNode)
                l.Add((VariableInitNode)node.Args);
            else if (node.Args is null || node.Args is EmptyNode)
                return l;
            else
                Error.ErrMessage(node.Args.MetaInf, "variable inizialization expected");

            return l;
        }

    }
}
