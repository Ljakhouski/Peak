using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.CodeGeneration
{
    class SemanticType
    {
        public enum Type
        {
            Bool,
            Int,
            Double,
            Str,
            Proc,
            Func,
            Object, // name of struct instance
            Array,
            Dict,
            RefOnMethodContext,
            RefOnContext,
        }
        public Type Value { get; set; }
        public SemanticType SecondValue { get; set; } // only for define type of array/struct/stack/dict...
        public List<SemanticType> Args { get; set; }
        public SemanticType ReturnType { get; set; } // only for procedure
        public SymbolTable ContextTable { get; internal set; }

        public override bool Equals(object obj)
        {
            if (obj is SemanticType)
            {
                SemanticType t = (SemanticType)obj;
                if (this.Value == t.Value)
                {
                    if (this.Value == Type.Proc) 
                    {
                        if (equalsArgs(this.Args, t.Args))
                            return true;
                        else
                            return false;

                    }
                    return true;
                }
                else
                    return false;
            }
            else
                return base.Equals(obj);
        }
        public static bool operator ==(SemanticType first, SemanticType second)
        {
            if (first as object == null)
                return false;

            return first.Equals(second);

        }
        public static bool operator !=(SemanticType first, SemanticType second)
        {
            return !(first == second);
            /*if (first == null)
                return true;

            return !first.Equals(second);
            */
        }
        public override string ToString()
        {
            switch (this.Value)
            {
                case Type.Int:
                    return "int";
                case Type.Double:
                    return "double";
                case Type.Str:
                    return "string";
                case Type.Bool:
                    return "bool";
                default:
                    throw new CompileException();
            }
        }

        public SemanticType(Type type)
        {
            this.Value = type;
        }

        public SemanticType(ConstValueNode node)
        {
            makeSemanticTypeForConst(node);
        }

        public SemanticType(Node node)
        {
            if (node is ConstValueNode)
                makeSemanticTypeForConst((ConstValueNode)node);
           // else if (node is ConstantNode)
           //     makeSemanticTypeForConstantNode((ConstantNode)node);
            else if (node is IdentifierNode)
                makeSemanticTypeForIdentifier((IdentifierNode)node);
            else if (node is MethodNode)
                makeSemanticTypeForMethod((MethodNode)node);
            //else if (node is null)
            else
                throw new CompileException();
        }

        private void makeSemanticTypeForMethod(MethodNode node)
        {
            this.Value = Type.Proc;
            this.Args = new List<SemanticType>();

            this.ReturnType = node.RetType is null? null : new SemanticType(node.RetType);
            if (node.Args is VariableInitNode)
            {
                this.Args.Add(new SemanticType(node.Args));
            }
            else if (node.Args is SequenceNode)
                foreach (Node n in (node.Args as SequenceNode).Sequence)
                {
                    if (n is VariableInitNode)
                    {
                        if ((n as VariableInitNode).Type != null)
                            this.Args.Add(new SemanticType((n as VariableInitNode).Type));
                        else
                            Error.ErrMessage((n as VariableInitNode).Name, "expected type");
                    }

                    else
                        Error.ErrMessage(n.MetaInf, "expected variable initialize");
                }
        }

        private void makeSemanticTypeForIdentifier(IdentifierNode node)
        {
            if (node.Id == "int")
            {
                this.Value = Type.Int;
            }
            else if (node.Id == "double")
            {
                this.Value = Type.Double;
            }
            else if (node.Id == "str")
            {
                this.Value = Type.Str;
            }
            else
                Error.ErrMessage(node.Id, "currently not supported");
        }

      /*  private void makeSemanticTypeForConstantNode(ConstantNode node)
        {
            if (node.Content == "int")
            {
                this.Value = Type.Int;
            }
            else if (node.Content == "double")
            {
                this.Value = Type.Double;
            }
            else if (node.Content == "str")
            {
                this.Value = Type.Str;
            }
            else
                throw new CompileException();
        }*/

        private void makeSemanticTypeForConst(ConstValueNode node)
        {
            if (node.Value.Type == type.IntValue)
            {
                this.Value = Type.Int;
            }
            else if (node.Value.Type == type.DoubleValue)
            {
                this.Value = Type.Double;
            }
            else if (node.Value.Type == type.StrValue)
            {
                this.Value = Type.Str;
            }
            else if (node.Value.Type == type.BoolValue)
            {
                this.Value = Type.Bool;
            }
            else
                throw new CompileException();
        }

        private bool equalsArgs(List<SemanticType> args1, List<SemanticType> args2)
        {
            if (args1.Count != args2.Count)
                return false;

            for (int i = 0; i < args1.Count; i++)
                if (args1[i] != args2[i])
                    return false;
            return true;
        }
    }
}
