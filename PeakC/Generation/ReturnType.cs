﻿using Peak.PeakC;
using System;
using System.Collections.Generic;
using System.Text;

namespace Peak.PeakC.Generation
{
    enum Type
    {
        Int,
        Double,
        Str,
        Bool,

        Method,
        IdType, // struct ot other data-type
        AnyToCompare,
        Struct,
        ContextRef,
    }
    class SemanticType
    {
        private Node typeNode;

        public bool IsNothing { get; set; } = false;

        public virtual Type Type { get; set; }
        public string TypeIdentifier { get; set; } // if type is 'IdType'

        public static bool operator == (SemanticType first, SemanticType second)
        {
            if (first is null)
            {
                if (second is null)
                    return true;
                else if (second.Type == Type.AnyToCompare)
                    return true;
                else
                    return false;
            }
            else if (second is null)
            {
                if (first is null)
                    return true;
                else if (first.Type == Type.AnyToCompare)
                    return true;
                else
                    return false;
            }
            else if (first.Type == second.Type)
                return true;
            else if (first.Type == Type.AnyToCompare || second.Type == Type.AnyToCompare)
                return true;
            else
                return false;
        }

        public static bool operator != (SemanticType first, SemanticType second)
        {
            return !(first == second);
        }

        public override bool Equals(object obj)
        {
            if (obj is SemanticType)
                return this == obj as SemanticType;
            else
                return false;
        }

        public SemanticType(Type type)
        {
            this.Type = type;
        }

        public SemanticType(ConstValueNode node)
        {

            if (node.Value.Type == PeakC.type.IntValue)
                this.Type = Type.Int;
            else if (node.Value.Type == PeakC.type.DoubleValue)
                this.Type = Type.Int;
            else if (node.Value.Type == PeakC.type.BoolValue)
                this.Type = Type.Bool;
            else
                throw new CompileException("it is other const value node type");
        }

        public SemanticType(Node node)
        {
            if (node is TypeNode)
            {
                //if ((node as TypeNode).)
                throw new CompileException("implementation of TypeNode is not define");
            }
            else if (node is IdentifierNode)
            {
                if ((node as IdentifierNode).Id.Content == "int")
                {
                    this.Type = Type.Int;
                }
                else if ((node as IdentifierNode).Id.Content == "double")
                {
                    this.Type = Type.Double;
                }
                else if ((node as IdentifierNode).Id.Content == "bool")
                {
                    this.Type = Type.Bool;
                }
                else
                {
                    this.Type = Type.IdType;
                    this.TypeIdentifier = (node as IdentifierNode).Id.Content;
                }
            }
        }

        public SemanticType()
        {
            //throw new CompileException("could not called?");
        }

    }

    class MethodSemanticType : SemanticType
    {
        public override Type Type {get{ return Type.Method; } }
        public List<SemanticType> Args { get; set; } = new List<SemanticType>();
        public SemanticType RetType { get; set; }

        public MethodContextReferenceType MethodContext { get; set; } = null;

        public static bool operator == (MethodSemanticType first, SemanticType second)
        {
            if (first is MethodSemanticType && second is MethodSemanticType
                && (first as MethodSemanticType).RetType == (second as MethodSemanticType).RetType
                )
            {
                int args1 = 0;
                int args2 = 0;

                if (first.Args is null == false)
                    args1 = first.Args.Count;
                if ((second as MethodSemanticType).Args is null == false)
                    args2 = (second as MethodSemanticType).Args.Count;

                if (args1 != args2)
                    return false;

                if (args1 > 0)
                {
                    for (int i = 0; i < (first as MethodSemanticType).Args.Count; i++)
                        if ((first as MethodSemanticType).Args[i] != (second as MethodSemanticType).Args[i])
                            return false;
                }

                return true;
            }
            else
                return false;
        }

        public static bool operator !=(MethodSemanticType first, SemanticType second)
        {
            return !(first == second);
        }

        public MethodSemanticType(MethodNode node)
        {
            this.Type = Type.Method;

            if (node.Args is SequenceNode)
                foreach (var e in (node.Args as SequenceNode).Sequence)
                    this.Args.Add(new SemanticType(e));
            else if (node.Args is EmptyNode || node.Args is null)
                this.Args = null;
            else
                this.Args.Add(new SemanticType(node.Args));

            if (node.RetType is null || node.RetType is EmptyNode)
            {
                this.RetType = null;
            }
            else
                this.RetType = new SemanticType(node.RetType);

        }

        public MethodSemanticType()
        {
            this.Type = Type.Method;
        }

    }
    class MethodContextReferenceType : SemanticType
    {
        public MethodSymbolTable CompareContext { get; set; }
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;
            else
            {
                if (obj is MethodContextReferenceType)
                {
                    return (obj as MethodContextReferenceType).CompareContext == this.CompareContext;
                }
                else
                    return false;
            }
        }
    }

}
