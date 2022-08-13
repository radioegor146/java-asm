using System;
using System.IO;
using BinaryEncoding;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.IO {
    public static class ClassFile {
        private const uint Magic = 0xCAFEBABE;

        internal static AttributeNode ParseAttribute(Stream stream, ClassReaderState state, AttributeScope scope) {
            AttributeNode attribute = new AttributeNode {
                Name = state.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String
            };
            attribute.Parse(stream, scope, state);
            return attribute;
        }

        private static FieldNode ParseField(Stream stream, ClassReaderState state) {
            FieldNode fieldNode = new FieldNode {
                Owner = state.ClassNode,

                Access = (FieldAccessModifiers) Binary.BigEndian.ReadUInt16(stream),
                Name = state.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String,
                Descriptor = TypeDescriptor.Parse(state.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String)
            };
            ushort attributesCount = Binary.BigEndian.ReadUInt16(stream);
            fieldNode.Attributes.Capacity = attributesCount;
            for (int i = 0; i < attributesCount; i++)
                fieldNode.Attributes.Add(ParseAttribute(stream, state, AttributeScope.Field));
            return fieldNode;
        }

        private static MethodNode ParseMethod(Stream stream, ClassReaderState state) {
            MethodNode methodNode = new MethodNode {
                Owner = state.ClassNode,

                Access = (MethodAccessModifiers) Binary.BigEndian.ReadUInt16(stream),
                Name = state.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String,
                Descriptor = MethodDescriptor.Parse(state.ConstantPool.GetEntry<Utf8Entry>(Binary.BigEndian.ReadUInt16(stream)).String)
            };
            ushort attributesCount = Binary.BigEndian.ReadUInt16(stream);
            methodNode.Attributes.Capacity = attributesCount;
            for (int i = 0; i < attributesCount; i++)
                methodNode.Attributes.Add(ParseAttribute(stream, state, AttributeScope.Method));
            return methodNode;
        }

        public static ClassNode ParseClass(Stream stream) {
            ClassReaderState state = new ClassReaderState();
            ClassNode result = new ClassNode();
            state.ClassNode = result;

            if (Binary.BigEndian.ReadUInt32(stream) != Magic)
                throw new IOException("Wrong magic in class");

            result.MinorVersion = Binary.BigEndian.ReadUInt16(stream);
            result.MajorVersion = (ClassVersion) Binary.BigEndian.ReadUInt16(stream);

            if (result.MajorVersion > ClassVersion.Java8)
                throw new Exception($"Wrong Java version: {result.MajorVersion}");

            ConstantPool constantPool = new ConstantPool();
            constantPool.Read(stream);
            state.ConstantPool = constantPool;

            result.Access = (ClassAccessModifiers) Binary.BigEndian.ReadUInt16(stream);

            result.Name = new ClassName(constantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(stream)).Name.String);
            result.SuperName = new ClassName(constantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(stream)).Name.String);

            ushort interfacesCount = Binary.BigEndian.ReadUInt16(stream);
            result.Interfaces.Capacity = interfacesCount;
            for (int i = 0; i < interfacesCount; i++)
                result.Interfaces.Add(new ClassName(constantPool.GetEntry<ClassEntry>(Binary.BigEndian.ReadUInt16(stream)).Name.String));

            ushort fieldsCount = Binary.BigEndian.ReadUInt16(stream);
            result.Fields.Capacity = fieldsCount;
            for (int i = 0; i < fieldsCount; i++)
                result.Fields.Add(ParseField(stream, state));

            ushort methodsCount = Binary.BigEndian.ReadUInt16(stream);
            result.Methods.Capacity = methodsCount;
            for (int i = 0; i < methodsCount; i++)
                result.Methods.Add(ParseMethod(stream, state));

            ushort attributesCount = Binary.BigEndian.ReadUInt16(stream);
            result.Attributes.Capacity = attributesCount;
            for (int i = 0; i < attributesCount; i++)
                result.Attributes.Add(ParseAttribute(stream, state, AttributeScope.Class));

            result.Parse(state);

            return result;
        }

        internal static void WriteAttribute(Stream stream, AttributeNode attribute, ClassWriterState state, AttributeScope scope) {
            Binary.BigEndian.Write(stream, state.ConstantPool.Find(new Utf8Entry(attribute.Name)));
            attribute.Data = attribute.ParsedAttribute?.Save(state, scope) ?? attribute.Data;
            if (attribute.Data.LongLength > uint.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(attribute.Data.LongLength), $"Attribute data length too big: {attribute.Data.LongLength} > {uint.MaxValue}");
            Binary.BigEndian.Write(stream, (uint) attribute.Data.LongLength);
            stream.Write(attribute.Data, 0, attribute.Data.Length);
        }

        private static void WriteField(Stream stream, FieldNode fieldNode, ClassWriterState state) {
            Binary.BigEndian.Write(stream, (ushort) fieldNode.Access);
            Binary.BigEndian.Write(stream, state.ConstantPool.Find(new Utf8Entry(fieldNode.Name)));
            Binary.BigEndian.Write(stream, state.ConstantPool.Find(new Utf8Entry(fieldNode.Descriptor.ToString())));
            if (fieldNode.Attributes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(fieldNode.Attributes.Count), $"Too many attributes: {fieldNode.Attributes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) fieldNode.Attributes.Count);
            foreach (AttributeNode attriute in fieldNode.Attributes)
                WriteAttribute(stream, attriute, state, AttributeScope.Field);
        }

        private static void WriteMethod(Stream stream, MethodNode methodNode, ClassWriterState state) {
            Binary.BigEndian.Write(stream, (ushort) methodNode.Access);
            Binary.BigEndian.Write(stream, state.ConstantPool.Find(new Utf8Entry(methodNode.Name)));
            Binary.BigEndian.Write(stream, state.ConstantPool.Find(new Utf8Entry(methodNode.Descriptor.ToString())));
            if (methodNode.Attributes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(methodNode.Attributes.Count), $"Too many attributes: {methodNode.Attributes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(stream, (ushort) methodNode.Attributes.Count);
            foreach (AttributeNode attriute in methodNode.Attributes)
                WriteAttribute(stream, attriute, state, AttributeScope.Method);
        }

        public static void WriteClass(Stream stream, ClassNode classNode) {
            Binary.BigEndian.Write(stream, Magic);
            Binary.BigEndian.Write(stream, classNode.MinorVersion);
            Binary.BigEndian.Write(stream, (ushort) classNode.MajorVersion);
            MemoryStream afterConstantPoolDataStream = new MemoryStream();
            ConstantPool constantPool = new ConstantPool();
            ClassWriterState state = new ClassWriterState {
                ClassNode = classNode,
                ConstantPool = constantPool
            };

            classNode.Save(state);

            Binary.BigEndian.Write(afterConstantPoolDataStream, (ushort) classNode.Access);
            Binary.BigEndian.Write(afterConstantPoolDataStream,
                constantPool.Find(new ClassEntry(new Utf8Entry(classNode.Name.Name))));
            Binary.BigEndian.Write(afterConstantPoolDataStream,
                constantPool.Find(new ClassEntry(new Utf8Entry(classNode.SuperName.Name))));

            if (classNode.Interfaces.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(classNode.Interfaces.Count), $"Too many interfaces: {classNode.Interfaces.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(afterConstantPoolDataStream, (ushort) classNode.Interfaces.Count);
            foreach (ClassName interfaceClassName in classNode.Interfaces)
                Binary.BigEndian.Write(afterConstantPoolDataStream,
                    constantPool.Find(new ClassEntry(new Utf8Entry(interfaceClassName.Name))));

            if (classNode.Fields.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(classNode.Fields.Count), $"Too many fields: {classNode.Fields.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(afterConstantPoolDataStream, (ushort) classNode.Fields.Count);
            foreach (FieldNode field in classNode.Fields)
                WriteField(afterConstantPoolDataStream, field, state);

            if (classNode.Methods.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(classNode.Methods.Count), $"Too many methods: {classNode.Methods.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(afterConstantPoolDataStream, (ushort) classNode.Methods.Count);
            foreach (MethodNode method in classNode.Methods)
                WriteMethod(afterConstantPoolDataStream, method, state);

            if (classNode.Attributes.Count > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(classNode.Attributes.Count), $"Too many attributes: {classNode.Attributes.Count} > {ushort.MaxValue}");
            Binary.BigEndian.Write(afterConstantPoolDataStream, (ushort) classNode.Attributes.Count);
            foreach (AttributeNode attriute in classNode.Attributes)
                WriteAttribute(afterConstantPoolDataStream, attriute, state, AttributeScope.Class);

            constantPool.Write(stream);
            byte[] data = afterConstantPoolDataStream.ToArray();
            stream.Write(data, 0, data.Length);
        }
    }
}