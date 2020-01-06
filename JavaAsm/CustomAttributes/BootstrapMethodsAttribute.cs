using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Instructions.Types;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class BootstrapMethod
    {
        public Handle BootstrapMethodReference { get; }

        public List<object> Arguments { get; } = new List<object>();

        public BootstrapMethod(Handle bootstrapMethodReference)
        {
            BootstrapMethodReference = bootstrapMethodReference;
        }

        public BootstrapMethod(Handle bootstrapMethodReference, List<object> arguments)
        {
            BootstrapMethodReference = bootstrapMethodReference;
            Arguments = arguments;
        }

        public bool Equals(BootstrapMethod other)
        {
            return BootstrapMethodReference.Equals(other.BootstrapMethodReference) && Arguments.Equals(other.Arguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BootstrapMethod)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BootstrapMethodReference.GetHashCode() * 397) ^ Arguments.GetHashCode();
            }
        }
    }


    internal class BootstrapMethodsAttribute : CustomAttribute
    {
        public List<BootstrapMethod> BootstrapMethods { get; set; } = new List<BootstrapMethod>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (BootstrapMethods.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Number of bootstrap methods is too big: {BootstrapMethods.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) BootstrapMethods.Count);
            foreach (var method in BootstrapMethods)
            {
                if (method.Arguments.Count > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(
                        $"Number of annotations is too big: {method.Arguments.Count} > {ushort.MaxValue}");
                Binary.BigEndian.Write(attributeDataStream, (ushort) method.Arguments.Count);
                foreach (var argument in method.Arguments)
                {
                    Binary.BigEndian.Write(attributeDataStream, writerState.ConstantPool.Find(argument switch
                    {
                        int integerValue => (Entry) new IntegerEntry(integerValue),
                        float floatValue => new FloatEntry(floatValue),
                        string stringValue => new StringEntry(new Utf8Entry(stringValue)),
                        long longValue => new LongEntry(longValue),
                        double doubleValue => new DoubleEntry(doubleValue),
                        Handle handle => handle.ToConstantPool(),
                        MethodDescriptor methodDescriptor => new MethodTypeEntry(new Utf8Entry(methodDescriptor.ToString())),
                        _ => throw new ArgumentOutOfRangeException($"Can't encode value of type {argument.GetType()}")
                    }));
                }
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class BootstrapMethodsAttributeFactory : ICustomAttributeFactory<BootstrapMethodsAttribute>
    {
        public BootstrapMethodsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var attribute = new BootstrapMethodsAttribute();

            var bootstrapMethodsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.BootstrapMethods.Capacity = bootstrapMethodsCount;
            for (var i = 0; i < bootstrapMethodsCount; i++)
            {
                var bootstrapMethod = new BootstrapMethod(
                    Handle.FromConstantPool(
                        readerState.ConstantPool.GetEntry<MethodHandleEntry>(
                            Binary.BigEndian.ReadUInt16(attributeDataStream))));
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

            return attribute;
        }
    }
}
