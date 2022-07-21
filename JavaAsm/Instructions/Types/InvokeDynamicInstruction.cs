using System;
using System.Collections.Generic;
using JavaAsm.Helpers;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.Instructions.Types
{
    public class InvokeDynamicInstruction : Instruction
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

    public class Handle
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
                    ? TypeDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                        .Descriptor.String)
                    : (IDescriptor) MethodDescriptor.Parse(methodHandleEntry.Reference.NameAndType
                        .Descriptor.String),
                Name = methodHandleEntry.Reference.NameAndType.Name.String,
                Owner = new ClassName(methodHandleEntry.Reference.Class.Name.String)
            };
        }

        internal MethodHandleEntry ToConstantPool()
        {
            ClassEntry referenceOwner = new ClassEntry(new Utf8Entry(this.Owner.Name));
            NameAndTypeEntry referenceNameAndType = new NameAndTypeEntry(new Utf8Entry(this.Name), new Utf8Entry(this.Descriptor.ToString()));
            ReferenceEntry reference;
            switch (this.Type) {
                case ReferenceKindType.GetField:
                    reference = new FieldReferenceEntry(referenceOwner, referenceNameAndType);
                    break;
                case ReferenceKindType.GetStatic:
                case ReferenceKindType.PutField:
                case ReferenceKindType.PutStatic:
                    reference = new FieldReferenceEntry(referenceOwner, referenceNameAndType);
                    break;
                case ReferenceKindType.InvokeVirtual:
                case ReferenceKindType.NewInvokeSpecial:
                case ReferenceKindType.InvokeStatic:
                case ReferenceKindType.InvokeSpecial:
                    reference = new MethodReferenceEntry(referenceOwner, referenceNameAndType);
                    break;
                case ReferenceKindType.InvokeReference:
                    reference = new InterfaceMethodReferenceEntry(referenceOwner, referenceNameAndType);
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(this.Type));
            }

            return new MethodHandleEntry(this.Type, reference);
        }
    }
}
