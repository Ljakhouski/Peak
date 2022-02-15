﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.CodeGeneration
{
    class SymbolType
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
        }
        public Type Value { get; set; }
        public SymbolType SecondValue { get; set; } // only for define type of array/struct/stack/dict...
        public List<SymbolType> Args { get; set; }
        public SymbolType ReturnType { get; set; } // only for procedure


        public override bool Equals(object obj)
        {
            if (obj is SymbolType)
            {
                SymbolType t = (SymbolType)obj;
                if (this.Value == t.Value)
                {
                    if (this.Value == Type.Proc) { throw new Exception(); }
                    return true;
                }
                else
                    return false;
            }
            else
                return base.Equals(obj);
        }
        public static bool operator ==(SymbolType first, SymbolType second)
        {
            if (first as object == null)
                return false;

            return first.Equals(second);

        }
        public static bool operator !=(SymbolType first, SymbolType second)
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
                    throw new Exception();
            }
        }

        public SymbolType(ConstValueNode node)
        {
            makeSymbolTypeForConst(node);
        }

        public SymbolType(Node node)
        {
            if (node is ConstValueNode)
                makeSymbolTypeForConst((ConstValueNode)node);
            else if (node is ConstantNode)
                makeSymbolTypeForConstantNode((ConstantNode)node);
            else if (node is IdentifierNode)
                makeSymbolTypeForIdentifier((IdentifierNode)node);
            else if (node is ProcedureNode)
                makeSymbolTypeForProcedure((ProcedureNode)node);
            else
                throw new Exception();
        }

        private void makeSymbolTypeForProcedure(ProcedureNode node)
        {
            this.Value = Type.Proc;
            this.Args = new List<SymbolType>();
            if (node.Args is VariableInitNode)
            {
                this.Args.Add(new SymbolType(node.Args));
            }
            else if (node.Args is SequenceNode)
                foreach (Node n in (node.Args as SequenceNode).Sequence)
                {
                    if (n is VariableInitNode)
                    {
                        if ((n as VariableInitNode).Type != null)
                            this.Args.Add(new SymbolType((n as VariableInitNode).Type));
                        else
                            Error.ErrMessage((n as VariableInitNode).Name, "expected type");
                    }

                    else
                        Error.ErrMessage(n.MetaInf, "expected variable initialize");
                }
        }

        private void makeSymbolTypeForIdentifier(IdentifierNode node)
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

        private void makeSymbolTypeForConstantNode(ConstantNode node)
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
                throw new Exception();
        }

        private void makeSymbolTypeForConst(ConstValueNode node)
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
            else
                throw new Exception();
        }
    }
}
