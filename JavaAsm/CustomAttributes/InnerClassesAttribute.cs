using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class InnerClassesAttribute : CustomAttribute {
        public List<InnerClass> Classes { get; set; } = new List<InnerClass>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            MemoryStream attributeDataStream = new MemoryStream();

            if (this.Classes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(this.Classes.Count), $"Too many inner classes: {this.Classes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(attributeDataStream, (ushort) this.Classes.Count);
            foreach (InnerClass innerClass in this.Classes) {
                Binary.BigEndian.Write(attributeDataStream,
                    writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(innerClass.InnerClassName.Name))));
                Binary.BigEndian.Write(attributeDataStream, innerClass.OuterClassName == null ? (ushort) 0 : writerState.ConstantPool.Find(new ClassEntry(new Utf8Entry(innerClass.OuterClassName.Name))));
                Binary.BigEndian.Write(attributeDataStream, innerClass.InnerName == null ? (ushort) 0 : writerState.ConstantPool.Find(new Utf8Entry(innerClass.InnerName)));
                Binary.BigEndian.Write(attributeDataStream, (ushort) innerClass.Access);
            }

            return attributeDataStream.ToArray();
        }
    }

    public class InnerClass {
        public ClassName InnerClassName { get; set; }

        public ClassName OuterClassName { get; set; }

        public string InnerName { get; set; }

        public ClassAccessModifiers Access { get; set; }

        public override string ToString() {
            return $"{AccessModifiersExtensions.ToString(this.Access)} {this.InnerClassName?.ToString() ?? "null"} {this.OuterClassName?.ToString() ?? "null"} {this.InnerName?.ToString() ?? "null"}";
        }
    }

    internal class InnerClassesAttributeFactory : ICustomAttributeFactory<InnerClassesAttribute> {
        public InnerClassesAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            InnerClassesAttribute attribute = new InnerClassesAttribute();

            ushort classesCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.Classes.Capacity = classesCount;
            for (int i = 0; i < classesCount; i++) {
                InnerClass innerClass = new InnerClass {
                    InnerClassName = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(attributeDataStream)).Name.String)
                };
                ushort outerClassIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);
                if (outerClassIndex != 0)
                    innerClass.OuterClassName = new ClassName(readerState.ConstantPool.GetEntry<ClassEntry>(outerClassIndex).Name.String);
                ushort innerNameIndex = Binary.BigEndian.ReadUInt16(attributeDataStream);
                if (innerNameIndex != 0)
                    innerClass.InnerName = readerState.ConstantPool.GetEntry<Utf8Entry>(innerNameIndex).String;
                innerClass.Access = (ClassAccessModifiers) Binary.BigEndian.ReadUInt16(attributeDataStream);
                attribute.Classes.Add(innerClass);
            }

            return attribute;
        }
    }
}