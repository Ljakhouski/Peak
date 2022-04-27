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
        private /*GenerationResult*/ void generateMethodDeclaration(MethodNode node, SymbolTable currentSymbolTable)
        {
            var type = new SymbolType(node);
            var symbols = currentSymbolTable.GetSymbols(node.Name);
            foreach (TableElement t in symbols)
            {
                if (t.Type.Value == SymbolType.Type.Proc)
                {
                    if (t.Type == type)
                        Error.ErrMessage(node.MetaInf, "name already exist");
                }
                else
                    Error.ErrMessage(node.MetaInf, "name already exist");
            }

            if (containsNativeModifier(node.Modifiers))
            {
                currentSymbolTable.Data.Add(new MethodTableElement()
                {
                    Info = node.MetaInf,
                    InfoNode = node,
                    Type = type,
                    Name = node.Name.Content,
                    
                    IsNative = true,
                    //MethodAddress = currentSymbolTable.CurrentRuntimeModule.Methods.Length}
                    NativeMethodName = node.Name.Content

                });

                if (node.Code != null)
                    Error.ErrMessage(node.MetaInf, "native method should't contains code");

                //return new GenerationResult() { Nothing = true };
            }
            else
            {
                /* prolog */
                // check if is the struct - method, then add a ref to struct
                // check if is the method in method, then add a ref to memory-scope
                // generate adding to local-vars-frame these refers
                // generate adding to local-vars-frame all args
                // generate a block of code 


                // context refs

                var structRef = getStructReferenceDefinition(currentSymbolTable);
                var methodRef = getMethodReferenceDefinition(currentSymbolTable);

                var procTable = new SymbolTable()
                {
                    CurrentMethod = new MethodDescription(),
                    CurrentRuntimeModule = currentSymbolTable.CurrentRuntimeModule,

                    IsMethodDefTable = true,
                    IsGlobalScope = false,
                    IsStructDefTable = false,

                    Prev = currentSymbolTable,
                   // MethodContextIndex = currentSymbolTable.CurrentRuntimeModule.Methods.Length

                };
                
                var method = procTable.CurrentMethod;

                if (structRef != null)
                {
                    procTable.RegisterSymbol(structRef);
                    addByteCode(InstructionName.Store, procTable.Data.Count - 1 , method);
                }

                if (methodRef != null)
                {
                    procTable.RegisterSymbol(methodRef);
                    addByteCode(InstructionName.Store, procTable.Data.Count - 1, method);
                }
                    
                // args
                if (node.Args is null == false && node.Args is EmptyNode == false)
                    if (node.Args is VariableInitNode)
                    {
                        var arg = node.Args as VariableInitNode;
                        procTable.RegisterSymbol(new TableElement() { Info = arg.MetaInf, Type = new SymbolType(arg.Type), InfoNode = arg });
                        addByteCode(generateStoreName(arg.Name, procTable), method);
                    }
                    else if (node.Args is SequenceNode)
                        foreach (Node arg__ in (node.Args as SequenceNode).Sequence)
                        {
                            if (arg__ is VariableInitNode)
                            {
                                var arg = arg__ as VariableInitNode;
                                procTable.RegisterSymbol(new TableElement() { Name = arg.Name.Content, Info = arg.MetaInf, Type = new SymbolType(arg.Type), InfoNode = arg });
                                addByteCode(generateStoreName(arg.Name, procTable), method);
                            }
                            else
                                Error.ErrMessage(arg__.MetaInf, "variable init expected");

                        }
                    else
                        Error.ErrMessage(node.MetaInf, "variable init expected");


                var methodElement = new MethodTableElement()
                {
                    Info = node.MetaInf,
                    InfoNode = node,
                    Type = type,
                    Name = node.Name.Content,
                    MethodContextTable = procTable,
                    IsNative = false,
                    MethodAddress = currentModule.Methods.Length,
                    NativeMethodName = node.Name.Content
                };
                currentSymbolTable.RegisterSymbol(methodElement);
                procTable.MethodElement = methodElement;

                generateForCodeBlock(node.Code, procTable);
                addByteCode(InstructionName.Return, method);
                //if (proc) delete from stack

                

                var methods = currentModule.Methods;
                method.LocalVarsArraySize = procTable.MemorySize;
                method.Name = methodElement.Name;
                Array.Resize(ref methods, methods.Length + 1);
                currentModule.Methods = methods; // !!!!!!
                currentModule.Methods[currentModule.Methods.Length - 1] = method;
            }
        }

        private TableElement getStructReferenceDefinition(SymbolTable table)
        {
            //if (table.IsMethodDefTable == false)
            //    throw new CompileException();
            if (table.IsGlobalScope)
                return null;

            SymbolTable t = table;
            while (t.Prev != null)
            {
                t = t.Prev;
                if (t.IsStructDefTable)
                {
                    return new TableElement() { Type = new SymbolType(SymbolType.Type.RefOnContext) { ContextTable = table.Prev }, MethodContextTable = table.Prev, ReferingContextId = t.Id };
                }
            }

            return null;
        }

        private TableElement getMethodReferenceDefinition(SymbolTable table)
        {
            //if (table.IsMethodDefTable == false)
            //    throw new CompileException();
            if (table.IsGlobalScope)
                return null;

            SymbolTable t = table;
            while (t.Prev != null)
            {
                
                if (t.IsMethodDefTable)
                {
                    return new TableElement() { Type = new SymbolType(SymbolType.Type.RefOnContext) { ContextTable = table.Prev }, MethodContextTable = table.Prev, ReferingContextId = t.Id };
                }
                t = t.Prev;
            }

            return null;
        }
        private bool containsNativeModifier(ModifierNode node)
        {
            if (node is null)
                return false;

            foreach (Token t in node.Modifiers)
                if (t == "native")
                    return true;
            return false;
        }


        private GenerationResult generateMethodCall(MethodCallNode node, SymbolTable callPlaceSymbolTable, SymbolTable argsCallPlaceSymbolTable)
        {
            var callArgs = new List<Node>();

            if (node.Args is null == false && node.Args is EmptyNode == false)
            if (node.Args is SequenceNode)
                callArgs.AddRange((node.Args as SequenceNode).Sequence);
            else
                callArgs.Add(node.Args);

            callArgs.Reverse(); // to reverse code-generation


            var argsType = new List<SymbolType>();
            var argsByteCode = new List<GenerationResult>();

            foreach (Node n in callArgs)
            {
                var gen = generateByteCode(n, argsCallPlaceSymbolTable);
                argsByteCode.Add(gen);
                argsType.Add(gen.ExprResult);
            }
            argsType.Reverse();

            var methods = callPlaceSymbolTable.GetSymbols(node.Id);

            foreach (TableElement method in methods)
            {
                if (method is MethodTableElement == false)
                    continue;

                var m = method as MethodTableElement;

                bool isWrong = false;
                for (int i = 0; i < m.Type.Args.Count; i++)
                {
                    if (m.Type.Args.Count != argsType.Count
                        ||
                    m.Type.Args[i] != argsType[i])
                    {
                        isWrong = true;
                        break;
                    }

                }
                if (isWrong == false)
                {
                    // make call

                    // NO NESTED CALLING! TODO: make it
                    var result = new GenerationResult();

                    result.ExprResult = m.Type.ReturnType;
                    result.Nothing = m.Type.ReturnType is null ? true : false;

                    // add args on stack
                    foreach (GenerationResult r in argsByteCode)
                        result.GeneratedByteCode.AddByteCode(r);

                    if (m.IsNative)
                    {
                        result.GeneratedByteCode.AddByteCode(InstructionName.PushConst, globalTable.GetConstantAddress(m.Name));
                        result.GeneratedByteCode.AddByteCode(InstructionName.CallNative);
                    }
                    else
                    {
                        result.GeneratedByteCode.AddByteCode(InstructionName.Call, m.MethodAddress);
                    }

                    return result;
                }

            }

            Error.ErrMessage(node.Id, "method with these arguments not exist");
            throw new CompileException();
        }

        //private bool EqualsMethodAndCall(string name, List<SymbolType> args, )
    }
}
