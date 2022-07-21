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

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            ushort value;
            switch (this.Value) {
                case long longValue:     value = writerState.ConstantPool.Find(new LongEntry(longValue)); break;
                case float floatValue:   value = writerState.ConstantPool.Find(new FloatEntry(floatValue)); break;
                case double doubleValue: value = writerState.ConstantPool.Find(new DoubleEntry(doubleValue)); break;
                case int integerValue:   value = writerState.ConstantPool.Find(new IntegerEntry(integerValue)); break;
                case string stringValue: value = writerState.ConstantPool.Find(new StringEntry(new Utf8Entry(stringValue))); break;
                default: throw new ArgumentOutOfRangeException(nameof(this.Value), $"Can't encode value of type {this.Value.GetType()}");
            }

            byte[] result = new byte[2];
            Binary.BigEndian.Set(value, result);
            return result;
        }
    }

    internal class ConstantValueAttributeFactory : ICustomAttributeFactory<ConstantValueAttribute>
    {
        public ConstantValueAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            Entry entry = readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream));
            object value;
            switch (entry) {
                case LongEntry longEntry:       value = longEntry.Value; break;
                case FloatEntry floatEntry:     value = floatEntry.Value; break;
                case DoubleEntry doubleEntry:   value = doubleEntry.Value; break;
                case IntegerEntry integerEntry: value = integerEntry.Value; break;
                case StringEntry stringEntry:   value = stringEntry.Value.String; break;
                default: throw new ArgumentOutOfRangeException(nameof(entry), $"Can't use constant pool entry of type {entry.GetType()} as constant value");
            }

            return new ConstantValueAttribute {Value = value};
        }
    }
}
