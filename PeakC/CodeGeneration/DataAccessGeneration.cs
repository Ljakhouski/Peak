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
        /*private GenerationResult generateIdAccessFromSymbolTable(Token name, SymbolTable currentSymbolTable)
        {
            SymbolTable currentContext = currentSymbolTable;

            while (true)
            {
                foreach (TableElement t in currentSymbolTable.Data)
                {
                    if (t.Name == name.Content)
                    {
                        //if (t.Type.Value == SymbolType.Type.Object) // for structures or namespaces

                        if (currentContext.IsGlobalScopeTable)
                        {
                            var code = new List<Instruction>()
                            {
                                new Instruction(){ Name = InstructionName.PushStatic, Operands = new int[]{ t.OffsetAddress} }
                            };
                            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                        }
                        else
                        {
                            var code = new List<Instruction>()
                            {
                                new Instruction(){ Name = InstructionName.Push, Operands = new int[]{ t.OffsetAddress} }
                            };
                            addByteCode(currentModule.Methods[currentModule.Methods.Length - 1], code);
                        }
                        return new GenerationResult() { Nothing = false, Result = t.Type };
                    }
                }

                /*if (currentContext.) // this is the function context and i can get a reference to another symbol-table with an access-code
                if (/*currentContext.Prev.IsGlobalScopeTable currentContext.IsGlobalScopeTable)
                {
                    Error.NameNotExistError(name);
                }
                /*else if (currentContext)
                else
                {
                    currentContext = currentContext.Prev;
                }
            }


        }*/

        private GenerationResult generateStoreData(Node node, SymbolTable currentSymbolTable)
        {
            if (node is IdentifierNode)
            {
                return generateStoreName((node as IdentifierNode).Id, currentSymbolTable);
            }
            else if (node is DotNode)
            {
                /* data-access-code-generation by dot-expression:
                    generate bytecode from right to left, last right node is store/push bytecode, all everything - data access (push)
                    
                 */
                throw new Exception();
            }
            /*else if (node is ArrayAccessNode)
            {
                var result = generatePushOnStackData(node.Left, currentSymbolTable);
                var indexResult = generatePushOnStackData(node.Index, currentSymbolTable);
            InstructionName.Store;
            }*/
            else
                Error.ErrMessage(node.MetaInf, "wrong expression");
            throw new Exception();
        }

        private GenerationResult generatePushOnStackData(Node node, SymbolTable currentSymbolTable)
        {
            if (node is IdentifierNode)
            {
                return generatePushOnStackName((node as IdentifierNode).Id, currentSymbolTable);
            }
            else if (node is DotNode)
            {
                /* data-access-code-generation by dot-expression:
                    generate bytecode from right to left, last right node is store/push bytecode, all everything - data access (push)
                    
                 */

                throw new Exception();
            }
            /*else if (node is ArrayAccessNode)
            {
                var result = generatePushOnStackData(node.Left, currentSymbolTable);
                var indexResult = generatePushOnStackData(node.Index, currentSymbolTable);
            InstructionName.Store;
            }*/
            else
                Error.ErrMessage(node.MetaInf, "wrong expression");
            throw new Exception();
        }

        /*private GenerationResult generateStoreName(Token name, SymbolTable currentSymbolTable)
        {
            if (currentSymbolTable.ContainsHere(name))
            {
                var element = currentSymbolTable.GetSymbol(name);

                if (currentSymbolTable.IsGlobalScopeTable)
                    addByteCode(InstructionName.StoreGlobal, element.OffsetAddress);
                else
                    addByteCode(InstructionName.Store, element.OffsetAddress);
                return new GenerationResult() { NameResult = element, Nothing = false, ExprResult = element.Type };
            }
            else if (currentSymbolTable.IsGlobalScopeTable)
            {
                Error.NameNotExistError(name);
                throw new Exception();
            }
            else
            {
                if (currentSymbolTable.IsMethodScope)
                {
                    var table = generatePreviousFrameContextAccess(currentSymbolTable);

                    return generateStoreName(name, table);
                }
                /*else if (currentSymbolTable.IsStructScope)
                else
                {
                    return generateStoreName(name, currentSymbolTable.Prev);
                }
            }
        }*/

        private GenerationResult generatePushOnStackName(Token name, SymbolTable currentSymbolTable, bool isTopFrameContext = false)
        {
            GenerationResult recursiveRefAccess(TableElement[] refs)
            {
                foreach (TableElement r in refs)
                {
                    if (r.MethodContextTable.ContainsHere(name))
                    {
                        var result = new GenerationResult() { Nothing = false };

                        if (isTopFrameContext)
                            result.GeneratedByteCode.AddByteCode(
                                InstructionName.Push, 
                                r.OffsetAddress
                                ); 
                        else
                            result.GeneratedByteCode.AddByteCode(
                                InstructionName.PushByRef,
                                r.OffsetAddress); // get ref from ref

                        result.GeneratedByteCode.AddByteCode(
                            InstructionName.PushByRef,
                            r.MethodContextTable.GetSymbolInMethodContext(name).OffsetAddress);

                        return result;
                    }
                    else
                    {
                        var res = recursiveRefAccess(r.MethodContextTable.GetContextRefs());
                        if (res.Nothing == false)
                        {
                            var result = new GenerationResult() { Nothing = false, ExprResult = res.ExprResult };

                            if (isTopFrameContext)
                                result.GeneratedByteCode.AddByteCode(InstructionName.Push, r.OffsetAddress);
                            else
                                result.GeneratedByteCode.AddByteCode(InstructionName.PushByRef, r.OffsetAddress); // get ref from ref

                            result.GeneratedByteCode.AddByteCode(res.GeneratedByteCode);

                            return result;
                        }
                    }
                    
                }
                return new GenerationResult() { Nothing = true };
            }

            /* search if is a method context (local method or global scope)*/
            if (currentSymbolTable.ContainsInMethodContext(name))
            {
                var element = currentSymbolTable.GetSymbolInMethodContext(name);
                var result = new GenerationResult() { NameResult = element, Nothing = false, ExprResult = element.Type };
                result.ExprResult = element.Type;
                
                if (currentSymbolTable.IsGlobalScope)
                    result.GeneratedByteCode.AddByteCode(InstructionName.PushGlobal, element.OffsetAddress);
                else
                    result.GeneratedByteCode.AddByteCode(InstructionName.Push, element.OffsetAddress);
                return result;
            }
            /* search if is a method context (only local scope in method)*/
            else if (currentSymbolTable.IsGlobalScope)
            {
                Error.NameNotExistError(name);
                throw new CompileException();
                //return new GenerationResult() { Nothing = true };
            }
            else
            {
                var refs = currentSymbolTable.GetContextRefs();

                if (refs.Length > 0)
                {
                    var res = recursiveRefAccess(refs);

                    if (res.Nothing == false)
                        return res;
                }

                // nothing in global function or nothing global function than this

                            /*** search in global ***/

                var t = currentSymbolTable.GetTableGlobalThanMethodTable();

                if (t.ContainsInMethodContext(name))
                {
                    var element = t.GetSymbolInMethodContext(name);
                    var result = new GenerationResult() { NameResult = element, Nothing = false, ExprResult = element.Type };
                    result.GeneratedByteCode.AddByteCode(InstructionName.PushGlobal, element.OffsetAddress);
                    return result;
                }
                else
                {
                    Error.NameNotExistError(name);
                    throw new CompileException();
                }
                
            }

        }


        private GenerationResult generateStoreName(Token name, SymbolTable currentSymbolTable)
        {
            GenerationResult recursiveRefAccess(TableElement[] refs, bool isTopFrameContext = false)
            {
                foreach (TableElement r in refs)
                {
                    if (r.MethodContextTable.ContainsHere(name))
                    {
                        var result = new GenerationResult() { Nothing = false };

                        if (isTopFrameContext)
                            result.GeneratedByteCode.AddByteCode(
                                InstructionName.Push,
                                r.OffsetAddress
                                );
                        else
                            result.GeneratedByteCode.AddByteCode(
                                InstructionName.PushByRef,
                                r.OffsetAddress); // get ref from ref

                        result.GeneratedByteCode.AddByteCode(
                            InstructionName.StoreByRef,
                            r.MethodContextTable.GetSymbolInMethodContext(name).OffsetAddress);

                        return result;
                    }
                    else
                    {
                        var res = recursiveRefAccess(r.MethodContextTable.GetContextRefs());
                        if (res.Nothing == false)
                        {
                            var result = new GenerationResult() { Nothing = false, ExprResult = res.ExprResult };

                            if (isTopFrameContext)
                                result.GeneratedByteCode.AddByteCode(InstructionName.Push, r.OffsetAddress);
                            else
                                result.GeneratedByteCode.AddByteCode(InstructionName.PushByRef, r.OffsetAddress); // get ref from ref

                            result.GeneratedByteCode.AddByteCode(res.GeneratedByteCode);

                            return result;
                        }
                    }

                }
                return new GenerationResult() { Nothing = true };
            }

            /* search if is a method context (local method or global scope)*/
            if (currentSymbolTable.ContainsInMethodContext(name))
            {
                var element = currentSymbolTable.GetSymbolInMethodContext(name);
                var result = new GenerationResult() { NameResult = element, Nothing = false, ExprResult = element.Type };
                result.ExprResult = element.Type;

                if (currentSymbolTable.IsGlobalScope)
                    result.GeneratedByteCode.AddByteCode(InstructionName.StoreGlobal, element.OffsetAddress);
                else
                    result.GeneratedByteCode.AddByteCode(InstructionName.Store, element.OffsetAddress);
                return result;
            }
            /* search if is a method context (only local scope in method)*/
            else if (currentSymbolTable.IsGlobalScope)
            {
                return new GenerationResult() { Nothing = true };
            }
            else
            {
                var refs = currentSymbolTable.GetContextRefs();

                if (refs.Length > 0)
                {
                    var res = recursiveRefAccess(refs, isTopFrameContext: true);

                    if (res.Nothing == false)
                        return res;
                }
                // nothing in global function or nothing global function than this

                /*** search in global ***/

                var t = currentSymbolTable.GetTableGlobalThanMethodTable();

                if (t.ContainsInMethodContext(name))
                {
                    var element = t.GetSymbolInMethodContext(name);
                    var result = new GenerationResult() { NameResult = element, Nothing = false, ExprResult = element.Type };
                    result.GeneratedByteCode.AddByteCode(InstructionName.StoreGlobal, element.OffsetAddress);
                    return result;
                }
                else
                {
                    return new GenerationResult() { Nothing = true };
                    //Error.NameNotExistError(name);
                    //throw new CompileException();
                }
            }

        }

        /*
                private GenerationResult generateStoreName(Token name, SymbolTable currentSymbolTable, bool isTopFrameContext = false)
                {
                    GenerationResult recursiveRefAccess(TableElement[] refs)
                    {
                        foreach (TableElement r in refs)
                        {
                            if (r.MethodContextTable.ContainsHere(name))
                            {
                                var result = new GenerationResult() { Nothing = false };

                                if (isTopFrameContext)
                                    result.GeneratedByteCode.AddByteCode(
                                        InstructionName.Push,
                                        r.OffsetAddress
                                        );
                                else
                                    result.GeneratedByteCode.AddByteCode(
                                        InstructionName.PushByRef,
                                        r.OffsetAddress); // get ref from ref

                                result.GeneratedByteCode.AddByteCode(
                                    InstructionName.PushByRef,
                                    r.MethodContextTable.GetSymbolInMethodContext(name).OffsetAddress);

                                return result;
                            }
                            else
                            {
                                var res = recursiveRefAccess(r.MethodContextTable.GetContextRefs());
                                if (res.Nothing == false)
                                {
                                    var result = new GenerationResult() { Nothing = false, ExprResult = res.ExprResult };

                                    if (isTopFrameContext)
                                        result.GeneratedByteCode.AddByteCode(InstructionName.Push, r.OffsetAddress);// TODO: write
                                    else
                                        result.GeneratedByteCode.AddByteCode(InstructionName.PushByRef, r.OffsetAddress); // get ref from ref

                                    result.GeneratedByteCode.AddByteCode(res.GeneratedByteCode);

                                    return result;
                                }
                            }

                        }
                        return new GenerationResult() { Nothing = true };
                    }



                    if (currentSymbolTable.Prev == null) // is global scope
                    {
                        if (globalTable.ContainsInMethodContext(name))
                        {
                            var element = globalTable.GetSymbolInMethodContext(name);
                            var res = new GenerationResult() { Nothing = false };
                            res.ExprResult = element.Type;
                            res.GeneratedByteCode.AddByteCode(
                                InstructionName.PushGlobal,
                                globalTable.GetSymbolInMethodContext(name).OffsetAddress
                                );
                            return res;
                        }
                        else
                            Error.NameNotExistError(name);
                        throw new CompileException();
                    }
                    else if (currentSymbolTable.ContainsInMethodContext(name))
                    {
                        var element = currentSymbolTable.GetSymbolInMethodContext(name);
                        var res = new GenerationResult() { Nothing = false };
                        res.ExprResult = element.Type;

                        if (currentSymbolTable.IsGlobalScope)
                            res.GeneratedByteCode.AddByteCode(InstructionName.PushGlobal, element.OffsetAddress);
                        else
                            res.GeneratedByteCode.AddByteCode(InstructionName.Push, element.OffsetAddress);
                        return new GenerationResult() { NameResult = element, Nothing = false, ExprResult = element.Type };
                    }
                    else
                    {
                        var refs = currentSymbolTable.GetContextRefs();
                        if (refs.Length > 0)
                        {
                            return recursiveRefAccess(refs);
                        }
                        else if (globalTable.ContainsInMethodContext(name))
                        {
                            // find first non method scope (gobal - if/while/gobal or other struct)
                            var table = currentSymbolTable.GetTableGlobalThanMethodTable();

                            if (table == null)
                            {
                                Error.NameNotExistError(name);
                                throw new CompileException();
                            }

                            var element = table.GetSymbolInMethodContext(name);
                            var res = new GenerationResult() { Nothing = false };
                            res.ExprResult = element.Type;

                            if (table.IsGlobalScope)
                                res.GeneratedByteCode.AddByteCode(
                                    InstructionName.PushGlobal,
                                    globalTable.GetSymbolInMethodContext(name).OffsetAddress
                                    );
                            else
                                throw new CompileException();

                            return res;
                        }
                        else
                            Error.NameNotExistError(name);
                        throw new CompileException();
                    }

                }*/
        /*private SymbolTable generatePreviousFrameContextAccess(SymbolTable currentSymbolTable)
        {
            foreach (TableElement element in currentSymbolTable.Data)
            {
                if (element.Type.Value == SymbolType.Type.RefOnMethodContext)
                {
                    var address = element.OffsetAddress;
                    addByteCode(InstructionName.Push, address);
                    return element.MethodContextTable;
                }
            }
            throw new Exception();
            //Error.ErrMessage();
        }*/

    }
}
