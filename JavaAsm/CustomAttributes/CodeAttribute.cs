using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class CodeAttribute : CustomAttribute {
        public ushort MaxStack { get; set; }

        public ushort MaxLocals { get; set; }

        public byte[] Code { get; set; }

        public class ExceptionTableEntry {
            public ushort StartPc { get; set; }

            public ushort EndPc { get; set; }

            public ushort HandlerPc { get; set; }

            public ClassName CatchType { get; set; }
        }

        public List<ExceptionTableEntry> ExceptionTable { get; set; } = new List<ExceptionTableEntry>();

        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            Binary.BigEndian.Write(attributeDataStream, this.MaxStack);
            Binary.BigEndian.Write(attributeDataStream, this.MaxLocals);

            if (this.Code.LongLength > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Code.LongLength), $"Code length too big: {this.Code.LongLength} > {uint.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (uint) this.Code.LongLength);
            attributeDataStream.Write(this.Code);

            if (this.ExceptionTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.ExceptionTable.Count), $"Exception table too big: {this.ExceptionTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.ExceptionTable.Count);
            foreach (ExceptionTableEntry exceptionTableEntry in this.ExceptionTable) {
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.StartPc);
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.EndPc);
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.HandlerPc);
                Binary.BigEndian.Write(attributeDataStream, (ushort) (exceptionTableEntry.CatchType == null ? 0 : writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(exceptionTableEntry.CatchType.Name)))));
            }

            if (this.Attributes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Attributes.Count), $"Too many attributes: {this.Attributes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.Attributes.Count);
            foreach (AttributeNode attriute in this.Attributes)
                ClassFile.WriteAttribute(attributeDataStream, attriute, writerState, AttributeScope.Code);

            return attributeDataStream.ToArray();
        }
    }

    internal class CodeAttributeFactory : ICustomAttributeFactory<CodeAttribute> {
        public CodeAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            ushort maxStack = Binary.BigEndian.ReadUInt16(attributeDataStream);
            ushort maxLocals = Binary.BigEndian.ReadUInt16(attributeDataStream);
            byte[] code = new byte[Binary.BigEndian.ReadUInt32(attributeDataStream)];
            attributeDataStream.Read(code);
            CodeAttribute attribute = new CodeAttribute {
                MaxStack = maxStack,
                MaxLocals = maxLocals,
                Code = code
            };

            ushort exceptionTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.ExceptionTable.Capacity = exceptionTableSize;
            for (int i = 0; i < exceptionTableSize; i++) {
                CodeAttribute.ExceptionTableEntry exceptionTableEntry = new CodeAttribute.ExceptionTableEntry {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    EndPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    HandlerPc = Binary.BigEndian.ReadUInt16(attributeDataStream)
                };
                ushort catchTypeIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);

                if (catchTypeIndex != 0) {
                    exceptionTableEntry.CatchType = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(catchTypeIndex).Name.String);
                }

                attribute.ExceptionTable.Add(exceptionTableEntry);
            }

            ushort attributesCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Attributes.Capacity = attributesCount;
            for (int i = 0; i < attributesCount; i++)
                attribute.Attributes.Add(ClassFile.ParseAttribute(attributeDataStream, readerState, AttributeScope.Code));

            return attribute;
        }
    }
}