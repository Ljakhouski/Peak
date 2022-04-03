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
        private /*GenerationResult*/ void generateProcDeclaration(ProcedureNode node, SymbolTable currentSymbolTable)
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


            var argsType = new List<SymbolType>();
            foreach (Node n in callArgs)
            {
                argsType.Add(generateByteCode(n, argsCallPlaceSymbolTable).ExprResult);
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
                    if (m.Type.Args[i] != argsType[i])
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
