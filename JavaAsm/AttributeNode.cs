using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.CustomAttributes;
using JavaAsm.Helpers;
using JavaAsm.IO;

namespace JavaAsm {
    public static class PredefinedAttributeNames {
        public const string AnnotationDefault = "AnnotationDefault";
        public const string BootstrapMethods = "BootstrapMethods";
        public const string Code = "Code";
        public const string ConstantValue = "ConstantValue";
        public const string Deprecated = "Deprecated";
        public const string EnclosingMethod = "EnclosingMethod";
        public const string Exceptions = "Exceptions";
        public const string InnerClasses = "InnerClasses";
        public const string LineNumberTable = "LineNumberTable";
        public const string LocalVariableTable = "LocalVariableTable";
        public const string LocalVariableTypeTable = "LocalVariableTypeTable";
        public const string MethodParameters = "MethodParameters";
        public const string RuntimeInvisibleAnnotations = "RuntimeInvisibleAnnotations";
        public const string RuntimeInvisibleParameterAnnotations = "RuntimeInvisibleParameterAnnotations";
        public const string RuntimeInvisibleTypeAnnotations = "RuntimeInvisibleTypeAnnotations";
        public const string RuntimeVisibleAnnotations = "RuntimeVisibleAnnotations";
        public const string RuntimeVisibleParameterAnnotations = "RuntimeVisibleParameterAnnotations";
        public const string RuntimeVisibleTypeAnnotations = "RuntimeVisibleTypeAnnotations";
        public const string Signature = "Signature";
        public const string SourceDebugExtension = "SourceDebugExtension";
        public const string SourceFile = "SourceFile";
        public const string StackMapTable = "StackMapTable";
        public const string Synthetic = "Synthetic";
    }

    public class AttributeNode {
        private static readonly Dictionary<(string Name, AttributeScope Scope), ICustomAttributeFactory<CustomAttribute>> predefinedAttributes
            = new Dictionary<(string Name, AttributeScope Scope), ICustomAttributeFactory<CustomAttribute>> {
                {(PredefinedAttributeNames.Code, AttributeScope.Method), new CodeAttributeFactory()},
                {(PredefinedAttributeNames.ConstantValue, AttributeScope.Field), new ConstantValueAttributeFactory()},
                {(PredefinedAttributeNames.SourceDebugExtension, AttributeScope.Class), new SourceDebugExtensionFactory()},
                {(PredefinedAttributeNames.SourceFile, AttributeScope.Class), new SourceFileAttributeFactory()},
                {(PredefinedAttributeNames.Exceptions, AttributeScope.Method), new ExceptionsAttributeFactory()},
                {(PredefinedAttributeNames.EnclosingMethod, AttributeScope.Class), new EnclosingMethodAttributeFactory()},
                {(PredefinedAttributeNames.Synthetic, AttributeScope.Class), new SyntheticAttributeFactory()},
                {(PredefinedAttributeNames.Synthetic, AttributeScope.Method), new SyntheticAttributeFactory()},
                {(PredefinedAttributeNames.Synthetic, AttributeScope.Field), new SyntheticAttributeFactory()},
                {(PredefinedAttributeNames.Signature, AttributeScope.Class), new SignatureAttributeFactory()},
                {(PredefinedAttributeNames.Signature, AttributeScope.Method), new SignatureAttributeFactory()},
                {(PredefinedAttributeNames.Signature, AttributeScope.Field), new SignatureAttributeFactory()},
                {(PredefinedAttributeNames.LineNumberTable, AttributeScope.Code), new LineNumberTableAttributeFactory()},
                {(PredefinedAttributeNames.Deprecated, AttributeScope.Class), new DeprecatedAttributeFactory()},
                {(PredefinedAttributeNames.Deprecated, AttributeScope.Method), new DeprecatedAttributeFactory()},
                {(PredefinedAttributeNames.Deprecated, AttributeScope.Field), new DeprecatedAttributeFactory()},
                {(PredefinedAttributeNames.MethodParameters, AttributeScope.Method), new MethodParametersAttributeFactory()},
                {(PredefinedAttributeNames.LocalVariableTable, AttributeScope.Code), new LocalVariableTableAttributeFactory()},
                {(PredefinedAttributeNames.LocalVariableTypeTable, AttributeScope.Code), new LocalVariableTypeTableAttributeFactory()},
                {(PredefinedAttributeNames.InnerClasses, AttributeScope.Class), new InnerClassesAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleAnnotations, AttributeScope.Class), new RuntimeInvisibleAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleAnnotations, AttributeScope.Method), new RuntimeInvisibleAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleAnnotations, AttributeScope.Field), new RuntimeInvisibleAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleAnnotations, AttributeScope.Class), new RuntimeVisibleAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleAnnotations, AttributeScope.Method), new RuntimeVisibleAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleAnnotations, AttributeScope.Field), new RuntimeVisibleAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleParameterAnnotations, AttributeScope.Method), new RuntimeInvisibleParameterAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleParameterAnnotations, AttributeScope.Method), new RuntimeVisibleParameterAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleTypeAnnotations, AttributeScope.Class), new RuntimeInvisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleTypeAnnotations, AttributeScope.Method), new RuntimeInvisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleTypeAnnotations, AttributeScope.Field), new RuntimeInvisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeInvisibleTypeAnnotations, AttributeScope.Code), new RuntimeInvisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleTypeAnnotations, AttributeScope.Class), new RuntimeVisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleTypeAnnotations, AttributeScope.Method), new RuntimeVisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleTypeAnnotations, AttributeScope.Field), new RuntimeVisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.RuntimeVisibleTypeAnnotations, AttributeScope.Code), new RuntimeVisibleTypeAnnotationsAttributeFactory()},
                {(PredefinedAttributeNames.AnnotationDefault, AttributeScope.Method), new AnnotationDefaultAttributeFactory()},
                {(PredefinedAttributeNames.BootstrapMethods, AttributeScope.Class), new BootstrapMethodsAttributeFactory()},
                {(PredefinedAttributeNames.StackMapTable, AttributeScope.Code), new StackMapTableAttributeFactory()}
            };

        public string Name { get; set; }

        public byte[] Data { get; set; }

        public CustomAttribute ParsedAttribute { get; set; }

        internal void Parse(Stream stream, AttributeScope scope, ClassReaderState readerState) {
            uint dataLength = Binary.BigEndian.ReadUInt32(stream);
            byte[] data = stream.ReadBytes(dataLength);

            try {
                if (!predefinedAttributes.ContainsKey((this.Name, scope)))
                    throw new ArgumentException($"Attribute {this.Name} in {scope} not found");
                ReadWriteCountStream readWriteCounter = new ReadWriteCountStream(new MemoryStream(data));
                this.ParsedAttribute = predefinedAttributes[(this.Name, scope)].Parse(readWriteCounter, dataLength, readerState, scope);
                if (readWriteCounter.ReadBytes != dataLength)
                    throw new ArgumentOutOfRangeException(nameof(dataLength),
                        $"Wrong data length of attribute {this.Name} in {scope}: Given {dataLength}, Read: {readWriteCounter.ReadBytes}");
            }
            catch {
                this.Data = data;
            }
        }
    }

    public abstract class CustomAttribute {
        internal abstract byte[] Save(ClassWriterState writerState, AttributeScope scope);
    }

    internal interface ICustomAttributeFactory<out T> where T : CustomAttribute {
        T Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope);
    }

    internal enum AttributeScope {
        Class,
        Method,
        Field,
        Code
    }
}