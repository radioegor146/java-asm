using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class CodeAttribute : CustomAttribute
    {
        public ushort MaxStack { get; set; }

        public ushort MaxLocals { get; set; }

        public byte[] Code { get; set; }

        public class ExceptionTableEntry
        {
            public ushort StartPc { get; set; }

            public ushort EndPc { get; set; }

            public ushort HandlerPc { get; set; }

            public ClassName CatchType { get; set; }
        }

        public List<ExceptionTableEntry> ExceptionTable { get; set; } = new List<ExceptionTableEntry>();

        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(); 

            Binary.BigEndian.Write(attributeDataStream, MaxStack);
            Binary.BigEndian.Write(attributeDataStream, MaxLocals);

            if (Code.LongLength > uint.MaxValue)
                throw new ArgumentOutOfRangeException($"Code length too big: {Code.LongLength} > {uint.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (uint) Code.LongLength);
            attributeDataStream.Write(Code);

            if (ExceptionTable.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Exception table too big: {ExceptionTable.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) ExceptionTable.Count);
            foreach (var exceptionTableEntry in ExceptionTable)
            {
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.StartPc);
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.EndPc);
                Binary.BigEndian.Write(attributeDataStream, exceptionTableEntry.HandlerPc);
                Binary.BigEndian.Write(attributeDataStream, (ushort) (exceptionTableEntry.CatchType == null ? 0 : 
                    writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(exceptionTableEntry.CatchType.Name)))));
            }

            if (Attributes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Too many attributes: {Attributes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) Attributes.Count);
            foreach (var attriute in Attributes)
                ClassFile.WriteAttribute(attributeDataStream, attriute, writerState, AttributeScope.Code);

            return attributeDataStream.ToArray();
        }
    }

    internal class CodeAttributeFactory : ICustomAttributeFactory<CodeAttribute>
    {
        public CodeAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var maxStack = Binary.BigEndian.ReadUInt16(attributeDataStream);
            var maxLocals = Binary.BigEndian.ReadUInt16(attributeDataStream);
            var code = new byte[Binary.BigEndian.ReadUInt32(attributeDataStream)];
            attributeDataStream.Read(code);
            var attribute = new CodeAttribute
            {
                MaxStack = maxStack,
                MaxLocals = maxLocals,
                Code = code
            };

            var exceptionTableSize = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.ExceptionTable.Capacity = exceptionTableSize;
            for (var i = 0; i < exceptionTableSize; i++)
            {
                var exceptionTableEntry = new CodeAttribute.ExceptionTableEntry
                {
                    StartPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    EndPc = Binary.BigEndian.ReadUInt16(attributeDataStream),
                    HandlerPc = Binary.BigEndian.ReadUInt16(attributeDataStream)
                };
                var catchTypeIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);

                if (catchTypeIndex != 0)
                {
                    exceptionTableEntry.CatchType = new ClassName(readerState.ConstantPool
                        .GetEntry<ClassEntry>(catchTypeIndex)
                        .Name.String);
                }

                attribute.ExceptionTable.Add(exceptionTableEntry);
            }

            var attributesCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Attributes.Capacity = attributesCount;
            for (var i = 0; i < attributesCount; i++)
                attribute.Attributes.Add(ClassFile.ParseAttribute(attributeDataStream, readerState, AttributeScope.Code));

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for Code attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
