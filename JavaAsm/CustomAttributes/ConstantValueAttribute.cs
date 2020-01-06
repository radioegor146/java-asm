using System;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes
{
    public class ConstantValueAttribute : CustomAttribute
    {
        public object Value { get; set; }

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            var result = new byte[2];
            Binary.BigEndian.Set(Value switch
            {
                long longValue => writerState.ConstantPool.Find(new LongEntry(longValue)),
                float floatValue => writerState.ConstantPool.Find(new FloatEntry(floatValue)),
                double doubleValue => writerState.ConstantPool.Find(new DoubleEntry(doubleValue)),
                int integerValue => writerState.ConstantPool.Find(new IntegerEntry(integerValue)),
                string stringValue => writerState.ConstantPool.Find(new StringEntry(new Utf8Entry(stringValue))),
                _ => throw new ArgumentOutOfRangeException(nameof(Value), $"Can't encode value of type {Value.GetType()}")
            }, result);
            return result;
        }
    }

    internal class ConstantValueAttributeFactory : ICustomAttributeFactory<ConstantValueAttribute>
    {
        public ConstantValueAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            var entry = readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream));
            return new ConstantValueAttribute {
                Value = entry switch
                    {
                        LongEntry longEntry => (longEntry.Value as object),
                        FloatEntry floatEntry => floatEntry.Value,
                        DoubleEntry doubleEntry => doubleEntry.Value,
                        IntegerEntry integerEntry => integerEntry.Value,
                        StringEntry stringEntry => stringEntry.Value.String,
                        _ => throw new ArgumentOutOfRangeException(nameof(entry),
                            $"Can't use constant pool entry of type {entry.GetType()} as constant value")
                    }
            };
        }
    }
}
