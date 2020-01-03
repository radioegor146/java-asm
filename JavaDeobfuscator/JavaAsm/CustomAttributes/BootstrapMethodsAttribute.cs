using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.Instructions.Types;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class BootstrapMethodsAttribute : CustomAttribute
    {
        public class BootstrapMethod
        {
            public Handle BootstrapMethodReference { get; set; }

            public List<object> Arguments { get; } = new List<object>();
        }

        public List<BootstrapMethod> BootstrapMethods { get; } = new List<BootstrapMethod>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            throw new NotImplementedException(); 
        }
    }

    internal class BootstrapMethodsAttributeFactory : ICustomAttributeFactory<BootstrapMethodsAttribute>
    {
        public BootstrapMethodsAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new BootstrapMethodsAttribute();

            var bootstrapMethodsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.BootstrapMethods.Capacity = bootstrapMethodsCount;
            for (var i = 0; i < bootstrapMethodsCount; i++)
            {
                var bootstrapMethod = new BootstrapMethodsAttribute.BootstrapMethod
                {
                    BootstrapMethodReference = Handle.FromConstantPool(readerState.ConstantPool.GetEntry<MethodHandleEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)))
                };
                var numberOfArguments = Binary.BigEndian.ReadUInt16(attributeDataStream);
                bootstrapMethod.Arguments.Capacity = numberOfArguments;
                for (var j = 0; j < numberOfArguments; j++)
                {
                    var argumentValueEntry =
                        readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream));
                    bootstrapMethod.Arguments.Add(argumentValueEntry switch
                    {
                        StringEntry stringEntry => stringEntry.Value.String,
                        ClassEntry classEntry => new ClassName(classEntry.Name.String),
                        IntegerEntry integerEntry => (object) integerEntry.Value,
                        LongEntry longEntry => longEntry.Value,
                        FloatEntry floatEntry => floatEntry.Value,
                        DoubleEntry doubleEntry => doubleEntry.Value,
                        MethodHandleEntry methodHandleEntry => Handle.FromConstantPool(methodHandleEntry),
                        MethodTypeEntry methodTypeEntry => MethodDescriptor.Parse(methodTypeEntry.Descriptor.String),
                        _ => throw new ArgumentException($"Entry of type {argumentValueEntry.Tag} is not supported for argument of bootstrap method")
                    });
                }
                attribute.BootstrapMethods.Add(bootstrapMethod);
            }

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for BootstrapMethods attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
