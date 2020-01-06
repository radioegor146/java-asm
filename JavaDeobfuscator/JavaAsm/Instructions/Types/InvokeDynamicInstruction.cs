using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.IO;
using JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class InvokeDynamicInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.INVOKEDYNAMIC;

        public string Name { get; set; }

        public MethodDescriptor Descriptor { get; set; }

        public Handle BootstrapMethod { get; set; }

        public List<object> BootstrapMethodArgs { get; set; }
    }

    public enum ReferenceKindType : byte
    {
        GetField = 1,
        GetStatic,
        PutField,
        PutStatic,
        InvokeVirtual,
        InvokeStatic,
        InvokeSpecial,
        NewInvokeSpecial,
        InvokeReference
    }

    internal class Handle
    {
        public ReferenceKindType Type { get; set; }

        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public IDescriptor Descriptor { get; set; }

        internal static Handle FromConstantPool(MethodHandleEntry methodHandleEntry)
        {
            return new Handle
            {
                Type = methodHandleEntry.ReferenceKind,
                Descriptor = methodHandleEntry.ReferenceKind.IsFieldReference()
                    ? (IDescriptor) TypeDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                        .Descriptor.String)
                    : (IDescriptor) MethodDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                        .Descriptor.String),
                Name = methodHandleEntry.Reference.NameAndType.Name.String,
                Owner = new ClassName(methodHandleEntry.Reference.Class.Name.String)
            };
        }

        internal MethodHandleEntry ToConstantPool()
        {
            var referenceOwner = new ClassEntry(new Utf8Entry(Owner.Name));
            var referenceNameAndType = new NameAndTypeEntry(new Utf8Entry(Name), new Utf8Entry(Descriptor.ToString()));
            var reference = Type switch
            {
                ReferenceKindType.GetField => (ReferenceEntry) new FieldReferenceEntry(referenceOwner,
                    referenceNameAndType),
                ReferenceKindType.GetStatic => new FieldReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.PutField => new FieldReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.PutStatic => new FieldReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.InvokeVirtual => new MethodReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.NewInvokeSpecial => new MethodReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.InvokeStatic => new MethodReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.InvokeSpecial => new MethodReferenceEntry(referenceOwner, referenceNameAndType),
                ReferenceKindType.InvokeReference => new InterfaceMethodReferenceEntry(referenceOwner,
                    referenceNameAndType),
                _ => throw new ArgumentOutOfRangeException(nameof(Type))
            };
            return new MethodHandleEntry(Type, reference);
        }
    }
}
