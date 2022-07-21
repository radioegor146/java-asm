using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.Helpers;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes.TypeAnnotation
{
    public class TypeAnnotationNode
    {
        public TargetType TargetType { get; set; }

        public TypeAnnotationTarget Target { get; set; }

        public TypePath TypePath { get; set; }

        public TypeDescriptor Type { get; set; }

        public class ElementValuePair
        {
            public string ElementName { get; set; }

            public ElementValue Value { get; set; }
        }

        public List<ElementValuePair> ElementValuePairs { get; set; } = new List<ElementValuePair>();

        internal static TypeAnnotationNode Parse(Stream stream, ClassReaderState readerState, AttributeScope scope)
        {
            TypeAnnotationNode typeAnnotation = new TypeAnnotationNode
            {
                TargetType = (TargetType) stream.ReadByteFully()
            };
            switch (typeAnnotation.TargetType) {
                case TargetType.GenericClassOrInterfaceDeclaration when scope == AttributeScope.Class:
                    typeAnnotation.Target = new TypeParameterTarget();
                    break;
                case TargetType.GenericMethodOrConstructorDeclaration when scope == AttributeScope.Method:
                    typeAnnotation.Target = new TypeParameterTarget();
                    break;
                case TargetType.ExtendsOrImplements when scope == AttributeScope.Class:
                    typeAnnotation.Target = new SupertypeTarget();
                    break;
                case TargetType.TypeInBoundInGenericClassOrInterface when scope == AttributeScope.Class:
                case TargetType.TypeInBoundInGenericMethodOrConstructor when scope == AttributeScope.Method:
                    typeAnnotation.Target = new TypeParameterBoundTarget();
                    break;
                case TargetType.FieldDeclaration when scope == AttributeScope.Field:
                case TargetType.ReturnTypeOrNewObject when scope == AttributeScope.Method:
                case TargetType.ReceiverTypeOfMethodOrConstructor when scope == AttributeScope.Method:
                    typeAnnotation.Target = new EmptyTarget();
                    break;
                case TargetType.TypeInFormalParameterOfMethodOrConstructorOrLambda when scope == AttributeScope.Method:
                    typeAnnotation.Target = new FormalParameterTarget();
                    break;
                case TargetType.ThrowsClause when scope == AttributeScope.Method:
                    typeAnnotation.Target = new ThrowsTarget();
                    break;
                case TargetType.LocalVariableDeclaration when scope == AttributeScope.Code:
                case TargetType.ResourceVariableDeclaration when scope == AttributeScope.Code:
                    typeAnnotation.Target = new LocalvarTarget();
                    break;
                case TargetType.ExceptionParameterDeclaration when scope == AttributeScope.Code:
                    typeAnnotation.Target = new CatchTarget();
                    break;
                case TargetType.InstanceOfExpression when scope == AttributeScope.Code:
                case TargetType.NewExpression when scope == AttributeScope.Code:
                case TargetType.MethodReferenceExpressionNew when scope == AttributeScope.Code:
                case TargetType.MethodReferenceExpressionIdentifier when scope == AttributeScope.Code:
                    typeAnnotation.Target = new OffsetTarget();
                    break;
                case TargetType.CastExpression when scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericConstructorInvocation when scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodInvocation when scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodReferenceExpressionNew when scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodReferenceExpressionIdentifier when scope == AttributeScope.Code:
                    typeAnnotation.Target = new TypeArgumentTarget();
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(TargetType));
            }

            typeAnnotation.Target.Read(stream, readerState);
            typeAnnotation.TypePath = new TypePath();
            typeAnnotation.TypePath.Read(stream, readerState);
            ushort elementValuePairsCount = Binary.BigEndian.ReadUInt16(stream);
            typeAnnotation.ElementValuePairs.Capacity = elementValuePairsCount;
            for (int i = 0; i < elementValuePairsCount; i++)
                typeAnnotation.ElementValuePairs.Add(new ElementValuePair
                {
                    ElementName = readerState.ConstantPool
                        .GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String,
                    Value = ElementValue.Parse(stream, readerState)
                });
            return typeAnnotation;
        }

        internal void Write(Stream stream, ClassWriterState writerState, AttributeScope scope)
        {
            stream.WriteByte((byte) this.TargetType);
            switch (this.TargetType)
            {
                case TargetType.GenericClassOrInterfaceDeclaration when this.Target.TargetTypeKind == TargetTypeKind.TypeParameter && scope == AttributeScope.Class:
                case TargetType.GenericMethodOrConstructorDeclaration when this.Target.TargetTypeKind == TargetTypeKind.TypeParameter && scope == AttributeScope.Method:
                case TargetType.ExtendsOrImplements when this.Target.TargetTypeKind == TargetTypeKind.Supertype && scope == AttributeScope.Class:
                case TargetType.TypeInBoundInGenericClassOrInterface when this.Target.TargetTypeKind == TargetTypeKind.TypeParameterBound && scope == AttributeScope.Class:
                case TargetType.TypeInBoundInGenericMethodOrConstructor when this.Target.TargetTypeKind == TargetTypeKind.TypeParameterBound && scope == AttributeScope.Method:
                case TargetType.FieldDeclaration when this.Target.TargetTypeKind == TargetTypeKind.Empty && scope == AttributeScope.Field:
                case TargetType.ReturnTypeOrNewObject when this.Target.TargetTypeKind == TargetTypeKind.Empty && scope == AttributeScope.Method:
                case TargetType.ReceiverTypeOfMethodOrConstructor when this.Target.TargetTypeKind == TargetTypeKind.Empty && scope == AttributeScope.Method:
                case TargetType.TypeInFormalParameterOfMethodOrConstructorOrLambda when this.Target.TargetTypeKind == TargetTypeKind.FormalParameter && scope == AttributeScope.Method:
                case TargetType.ThrowsClause when this.Target.TargetTypeKind == TargetTypeKind.Throws && scope == AttributeScope.Method:
                case TargetType.LocalVariableDeclaration when this.Target.TargetTypeKind == TargetTypeKind.Localvar && scope == AttributeScope.Code:
                case TargetType.ResourceVariableDeclaration when this.Target.TargetTypeKind == TargetTypeKind.Localvar && scope == AttributeScope.Code:
                case TargetType.ExceptionParameterDeclaration when this.Target.TargetTypeKind == TargetTypeKind.Catch && scope == AttributeScope.Code:
                case TargetType.InstanceOfExpression when this.Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.NewExpression when this.Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.MethodReferenceExpressionNew when this.Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.MethodReferenceExpressionIdentifier when this.Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.CastExpression when this.Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericConstructorInvocation when this.Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodInvocation when this.Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodReferenceExpressionNew when this.Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodReferenceExpressionIdentifier when this.Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                    this.Target.Write(stream, writerState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(this.TargetType));
            }

            this.TypePath.Write(stream, writerState);

            if (this.ElementValuePairs.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.ElementValuePairs.Count),
                    $"Too many ElementValues: {this.ElementValuePairs.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) this.ElementValuePairs.Count);
            foreach (ElementValuePair elementValuePair in this.ElementValuePairs)
            {
                Binary.BigEndian.Write(stream,
                    writerState.ConstantPool.Find(new Utf8Entry(elementValuePair.ElementName)));
                elementValuePair.Value.Write(stream, writerState);
            }
        }
    }
}
