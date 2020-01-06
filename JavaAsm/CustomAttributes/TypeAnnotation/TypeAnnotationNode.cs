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
            var typeAnnotation = new TypeAnnotationNode
            {
                TargetType = (TargetType) stream.ReadByteFully()
            };
            typeAnnotation.Target = typeAnnotation.TargetType switch
            {
                TargetType.GenericClassOrInterfaceDeclaration when scope == AttributeScope.Class => (TypeAnnotationTarget) new TypeParameterTarget(),
                TargetType.GenericMethodOrConstructorDeclaration when scope == AttributeScope.Method => new TypeParameterTarget(),
                TargetType.ExtendsOrImplements when scope == AttributeScope.Class => new SupertypeTarget(),
                TargetType.TypeInBoundInGenericClassOrInterface when scope == AttributeScope.Class => new TypeParameterBoundTarget(),
                TargetType.TypeInBoundInGenericMethodOrConstructor when scope == AttributeScope.Method => new TypeParameterBoundTarget(),
                TargetType.FieldDeclaration when scope == AttributeScope.Field => new EmptyTarget(),
                TargetType.ReturnTypeOrNewObject when scope == AttributeScope.Method => new EmptyTarget(),
                TargetType.ReceiverTypeOfMethodOrConstructor when scope == AttributeScope.Method => new EmptyTarget(),
                TargetType.TypeInFormalParameterOfMethodOrConstructorOrLambda when scope == AttributeScope.Method => new FormalParameterTarget(),
                TargetType.ThrowsClause when scope == AttributeScope.Method => new ThrowsTarget(),
                TargetType.LocalVariableDeclaration when scope == AttributeScope.Code => new LocalvarTarget(),
                TargetType.ResourceVariableDeclaration when scope == AttributeScope.Code => new LocalvarTarget(),
                TargetType.ExceptionParameterDeclaration when scope == AttributeScope.Code => new CatchTarget(),
                TargetType.InstanceOfExpression when scope == AttributeScope.Code => new OffsetTarget(),
                TargetType.NewExpression when scope == AttributeScope.Code => new OffsetTarget(),
                TargetType.MethodReferenceExpressionNew when scope == AttributeScope.Code => new OffsetTarget(),
                TargetType.MethodReferenceExpressionIdentifier when scope == AttributeScope.Code => new OffsetTarget(),
                TargetType.CastExpression when scope == AttributeScope.Code => new TypeArgumentTarget(),
                TargetType.ArgumentForGenericConstructorInvocation when scope == AttributeScope.Code => new TypeArgumentTarget(),
                TargetType.ArgumentForGenericMethodInvocation when scope == AttributeScope.Code => new TypeArgumentTarget(),
                TargetType.ArgumentForGenericMethodReferenceExpressionNew when scope == AttributeScope.Code => new TypeArgumentTarget(),
                TargetType.ArgumentForGenericMethodReferenceExpressionIdentifier when scope == AttributeScope.Code => new TypeArgumentTarget(),
                _ => throw new ArgumentOutOfRangeException(nameof(TargetType))
            };
            typeAnnotation.Target.Read(stream, readerState);
            typeAnnotation.TypePath = new TypePath();
            typeAnnotation.TypePath.Read(stream, readerState);
            var elementValuePairsCount = Binary.BigEndian.ReadUInt16(stream);
            typeAnnotation.ElementValuePairs.Capacity = elementValuePairsCount;
            for (var i = 0; i < elementValuePairsCount; i++)
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
            stream.WriteByte((byte) TargetType);
            switch (TargetType)
            {
                case TargetType.GenericClassOrInterfaceDeclaration when Target.TargetTypeKind == TargetTypeKind.TypeParameter && scope == AttributeScope.Class:
                case TargetType.GenericMethodOrConstructorDeclaration when Target.TargetTypeKind == TargetTypeKind.TypeParameter && scope == AttributeScope.Method:
                case TargetType.ExtendsOrImplements when Target.TargetTypeKind == TargetTypeKind.Supertype && scope == AttributeScope.Class:
                case TargetType.TypeInBoundInGenericClassOrInterface when Target.TargetTypeKind == TargetTypeKind.TypeParameterBound && scope == AttributeScope.Class:
                case TargetType.TypeInBoundInGenericMethodOrConstructor when Target.TargetTypeKind == TargetTypeKind.TypeParameterBound && scope == AttributeScope.Method:
                case TargetType.FieldDeclaration when Target.TargetTypeKind == TargetTypeKind.Empty && scope == AttributeScope.Field:
                case TargetType.ReturnTypeOrNewObject when Target.TargetTypeKind == TargetTypeKind.Empty && scope == AttributeScope.Method:
                case TargetType.ReceiverTypeOfMethodOrConstructor when Target.TargetTypeKind == TargetTypeKind.Empty && scope == AttributeScope.Method:
                case TargetType.TypeInFormalParameterOfMethodOrConstructorOrLambda when Target.TargetTypeKind == TargetTypeKind.FormalParameter && scope == AttributeScope.Method:
                case TargetType.ThrowsClause when Target.TargetTypeKind == TargetTypeKind.Throws && scope == AttributeScope.Method:
                case TargetType.LocalVariableDeclaration when Target.TargetTypeKind == TargetTypeKind.Localvar && scope == AttributeScope.Code:
                case TargetType.ResourceVariableDeclaration when Target.TargetTypeKind == TargetTypeKind.Localvar && scope == AttributeScope.Code:
                case TargetType.ExceptionParameterDeclaration when Target.TargetTypeKind == TargetTypeKind.Catch && scope == AttributeScope.Code:
                case TargetType.InstanceOfExpression when Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.NewExpression when Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.MethodReferenceExpressionNew when Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.MethodReferenceExpressionIdentifier when Target.TargetTypeKind == TargetTypeKind.Offset && scope == AttributeScope.Code:
                case TargetType.CastExpression when Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericConstructorInvocation when Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodInvocation when Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodReferenceExpressionNew when Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                case TargetType.ArgumentForGenericMethodReferenceExpressionIdentifier when Target.TargetTypeKind == TargetTypeKind.TypeArgument && scope == AttributeScope.Code:
                    Target.Write(stream, writerState);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(TargetType));
            }

            TypePath.Write(stream, writerState);

            if (ElementValuePairs.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(ElementValuePairs.Count),
                    $"Too many ElementValues: {ElementValuePairs.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort)ElementValuePairs.Count);
            foreach (var elementValuePair in ElementValuePairs)
            {
                Binary.BigEndian.Write(stream,
                    writerState.ConstantPool.Find(new Utf8Entry(elementValuePair.ElementName)));
                elementValuePair.Value.Write(stream, writerState);
            }
        }
    }
}
