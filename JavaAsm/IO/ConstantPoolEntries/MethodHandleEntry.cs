using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using BinaryEncoding;
using JavaAsm.Helpers;
using JavaAsm.Instructions.Types;

namespace JavaAsm.IO.ConstantPoolEntries
{
    internal class MethodHandleEntry : Entry
    {
        public ReferenceKindType ReferenceKind { get; }

        public ReferenceEntry Reference { get; private set; }
        private ushort referenceIndex;

        public MethodHandleEntry(ReferenceKindType referenceKind, ReferenceEntry reference)
        {
            ReferenceKind = referenceKind;
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        public MethodHandleEntry(Stream stream)
        {
            ReferenceKind = (ReferenceKindType) stream.ReadByteFully();
            referenceIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.MethodHandle;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            switch (ReferenceKind)
            {
                case ReferenceKindType.GetField:
                case ReferenceKindType.GetStatic:
                case ReferenceKindType.PutField:
                case ReferenceKindType.PutStatic:
                    Reference = constantPool.GetEntry<FieldReferenceEntry>(referenceIndex);
                    break;
                case ReferenceKindType.InvokeVirtual:
                case ReferenceKindType.NewInvokeSpecial:
                    Reference = constantPool.GetEntry<MethodReferenceEntry>(referenceIndex);
                    break;
                case ReferenceKindType.InvokeStatic:
                case ReferenceKindType.InvokeSpecial:
                    try
                    {
                        Reference = constantPool.GetEntry<MethodReferenceEntry>(referenceIndex);
                    }
                    catch (InvalidCastException)
                    {
                        Reference = constantPool.GetEntry<InterfaceMethodReferenceEntry>(referenceIndex);
                    }
                    break;
                case ReferenceKindType.InvokeReference:
                    Reference = constantPool.GetEntry<InterfaceMethodReferenceEntry>(referenceIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ReferenceKind));
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte)ReferenceKind);
            Binary.BigEndian.Write(stream, referenceIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            referenceIndex = constantPool.Find(Reference);
        }

        private bool Equals(MethodHandleEntry other)
        {
            return ReferenceKind == other.ReferenceKind && Equals(Reference, other.Reference);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MethodHandleEntry)obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)ReferenceKind * 397) ^ (Reference != null ? Reference.GetHashCode() : 0);
            }
        }
    }
}