using System;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class ConstantValueAttribute : CustomAttribute
    {
        public object Value { get; set; }

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            throw new NotImplementedException();
        }
    }

    internal class ConstantValueAttributeFactory : ICustomAttributeFactory<ConstantValueAttribute>
    {
        public ConstantValueAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            if (attributeNode.Data.Length != sizeof(ushort))
                throw new ArgumentOutOfRangeException($"Attribute length is incorrect for ConstantValue: {attributeNode.Data.Length} != {sizeof(ushort)}");
            var entry = readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.GetUInt16(attributeNode.Data));
            return new ConstantValueAttribute {
                Value = entry switch
                    {
                        LongEntry longEntry => (longEntry.Value as object),
                        FloatEntry floatEntry => floatEntry.Value,
                        DoubleEntry doubleEntry => doubleEntry.Value,
                        IntegerEntry integerEntry => integerEntry.Value,
                        StringEntry stringEntry => stringEntry.Value.String,
                        _ => throw new ArgumentOutOfRangeException(
                            $"Can't use constant pool entry of type {entry.GetType()} as constant value")
                    }
            };
        }
    }
}
