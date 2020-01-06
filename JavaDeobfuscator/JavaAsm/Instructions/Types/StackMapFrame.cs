using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.CustomAttributes;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal enum VerificationElementType
    {
        Top,
        Integer,
        Float,
        Long,
        Double,
        Null,
        UnitializedThis,
        Object,
        Unitialized
    }

    internal abstract class VerificationElement
    {
        public abstract VerificationElementType Type { get; }
    }

    internal class SimpleVerificationElement : VerificationElement
    {
        public override VerificationElementType Type { get; }

        public SimpleVerificationElement(VerificationElementType type)
        {
            type.CheckInAndThrow(nameof(type), VerificationElementType.Top, VerificationElementType.Integer,
                VerificationElementType.Float, VerificationElementType.Long, VerificationElementType.Double, VerificationElementType.Null, VerificationElementType.UnitializedThis);
            Type = type;
        }
    }

    internal class ObjectVerificationElement : VerificationElement
    {
        public override VerificationElementType Type => VerificationElementType.Object;

        public ClassName ObjectClass { get; set; }
    }

    internal class UninitializedVerificationElement : VerificationElement
    {
        public override VerificationElementType Type => VerificationElementType.Unitialized;

        public TypeInstruction NewInstruction { get; set; }
    }

    internal enum FrameType
    {
        Same,
        SameLocals1StackItem,
        SameLocals1StackItemExtended,
        Chop,
        SameExtended,
        Append,
        Full
    }

    internal class StackMapFrame : Instruction
    {
        public override Opcode Opcode => Opcode.None;

        public FrameType Type { get; set; }

        public ushort OffsetDelta { get; set; }

        public List<VerificationElement> Stack { get; } = new List<VerificationElement>();

        public List<VerificationElement> Locals { get; } = new List<VerificationElement>();

        public int? ChopK { get; set; }

        public override string ToString()
        {
            return "STACKFRAME";
        }
    }
}
