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
            this.ReferenceKind = referenceKind;
            this.Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        public MethodHandleEntry(Stream stream)
        {
            this.ReferenceKind = (ReferenceKindType) stream.ReadByteFully();
            this.referenceIndex = Binary.BigEndian.ReadUInt16(stream);
        }

        public override EntryTag Tag => EntryTag.MethodHandle;

        public override void ProcessFromConstantPool(ConstantPool constantPool)
        {
            switch (this.ReferenceKind)
            {
                case ReferenceKindType.GetField:
                case ReferenceKindType.GetStatic:
                case ReferenceKindType.PutField:
                case ReferenceKindType.PutStatic:
                    this.Reference = constantPool.GetEntry<FieldReferenceEntry>(this.referenceIndex);
                    break;
                case ReferenceKindType.InvokeVirtual:
                case ReferenceKindType.NewInvokeSpecial:
                    this.Reference = constantPool.GetEntry<MethodReferenceEntry>(this.referenceIndex);
                    break;
                case ReferenceKindType.InvokeStatic:
                case ReferenceKindType.InvokeSpecial:
                    try
                    {
                        this.Reference = constantPool.GetEntry<MethodReferenceEntry>(this.referenceIndex);
                    }
                    catch (InvalidCastException)
                    {
                        this.Reference = constantPool.GetEntry<InterfaceMethodReferenceEntry>(this.referenceIndex);
                    }
                    break;
                case ReferenceKindType.InvokeReference:
                    this.Reference = constantPool.GetEntry<InterfaceMethodReferenceEntry>(this.referenceIndex);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(this.ReferenceKind));
            }
        }

        public override void Write(Stream stream)
        {
            stream.WriteByte((byte) this.ReferenceKind);
            Binary.BigEndian.Write(stream, this.referenceIndex);
        }

        public override void PutToConstantPool(ConstantPool constantPool)
        {
            this.referenceIndex = constantPool.Find(this.Reference);
        }

        private bool Equals(MethodHandleEntry other)
        {
            return this.ReferenceKind == other.ReferenceKind && Equals(this.Reference, other.Reference);
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
                return ((int) this.ReferenceKind * 397) ^ (this.Reference != null ? this.Reference.GetHashCode() : 0);
            }
        }
    }
}