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
        private GenerationResult generateProcDeclaration(ProcedureNode node, SymbolTable currentSymbolTable)
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

            if (containtNativeModifier(node.Modifiers))
            {
                currentSymbolTable.Data.Add(new TableElement()
                {
                    Info = node.MetaInf,
                    InfoNode = node,
                    Type = type,
                    Name = node.Name.Content,
                    MethodInfo = new MethodElement()
                    {
                        IsNative = true,
                        //MethodAddress = currentSymbolTable.CurrentRuntimeModule.Methods.Length}
                        NativeMethodName = node.Name.Content
                    }
                });

                if (node.Code != null)
                    Error.ErrMessage(node.MetaInf, "native method should't contains code");

                return new GenerationResult() { Nothing = true };
            }
            else
                throw new Exception();
        }

        private bool containtNativeModifier(ModifierNode node)
        {
            foreach (Token t in node.Modifiers)
                if (t == "native")
                    return true;
            return false;
        }


        private GenerationResult generateMethodCall(MethodCallNode node, SymbolTable callPlaceSymbolTable, SymbolTable argsCallPlaceSymbolTable)
        {
            var callArgs = new List<Node>();

            if (node.Args is SequenceNode)
                callArgs.AddRange((node.Args as SequenceNode).Sequence);
            else
                callArgs.Add(node.Args);

            callArgs.Reverse(); // to reverse code-generation


            var argsType = new List<GenerationResult>();
            foreach (Node n in callArgs)
            {
                argsType.Add(generateByteCode(n, argsCallPlaceSymbolTable));
            }
            argsType.Reverse();

            var methods = callPlaceSymbolTable.GetSymbols(node.Id);

            foreach (TableElement m in methods)
            {
                bool isWrong = false;
                for (int i = 0; i < m.Type.Args.Count - 1; i++)
                {
                    if (m.Type.Args[i] != argsType[i].ExprResult)
                    {
                        isWrong = true;
                        break;
                    }
                        
                }
                if (isWrong == false)
                {
                    // make call

                    // NO NESTED CALLING! TODO: make it

                    addByteCode(InstructionName.PushConst, globalTable.GetConstantAddress(node.Id.Content));
                    addByteCode(InstructionName.CallNative);

                    return new GenerationResult() { Nothing = true };
                }

            }

            Error.ErrMessage(node.Id, "method not exist");
            throw new CompileException();
        }
    }
}
