using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class InnerClassesAttribute : CustomAttribute
    {
        public List<InnerClass> Classes { get; set; } = new List<InnerClass>();

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            if (Classes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException($"Local variable table is too big: {Classes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort)Classes.Count);
            foreach (var innerClass in Classes)
            {
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(innerClass.InnerClassName.Name))));
                Binary.BigEndian.Write(attributeDataStream, innerClass.OuterClassName == null ? (ushort) 0 :
                    writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(innerClass.OuterClassName.Name))));
                Binary.BigEndian.Write(attributeDataStream, innerClass.InnerName == null ? (ushort) 0 :
                    writerState.ConstantPool.Find(new Utf8Entry(innerClass.InnerName)));
                Binary.BigEndian.Write(attributeDataStream, (ushort) innerClass.Access);
            }

            return attributeDataStream.ToArray();
        }
    }

    internal class InnerClass
    {
        public ClassName InnerClassName { get; set; }

        public ClassName OuterClassName { get; set; }

        public string InnerName { get; set; }

        public AccessModifiers Access { get; set; }
    }

    internal class InnerClassesAttributeFactory : ICustomAttributeFactory<InnerClassesAttribute>
    {
        public InnerClassesAttribute Parse(AttributeNode attributeNode, ClassReaderState readerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream(attributeNode.Data);
            var attribute = new InnerClassesAttribute();

            var classesCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Classes.Capacity = classesCount;
            for (var i = 0; i < classesCount; i++)
            {
                var innerClass = new InnerClass
                {
                    InnerClassName = new ClassName(readerState.ConstantPool
                        .GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).Name.String)
                };
                var outerClassIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);
                if (outerClassIndex != 0)
                    innerClass.OuterClassName = new ClassName(readerState.ConstantPool
                        .GetEntry<ClassEntry>(outerClassIndex).Name.String);
                var innerNameIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);
                if (innerNameIndex != 0)
                    innerClass.InnerName = readerState.ConstantPool
                        .GetEntry<Utf8Entry>(innerNameIndex).String;
                innerClass.Access = (AccessModifiers) Binary.BigEndian.ReadUInt16(attributeDataStream);
                attribute.Classes.Add(innerClass);

            }

            if (attributeDataStream.Position != attributeDataStream.Length)
                throw new ArgumentOutOfRangeException(
                    $"Too many bytes for InnerClasses attribute: {attributeDataStream.Length} > {attributeDataStream.Position}");

            return attribute;
        }
    }
}
