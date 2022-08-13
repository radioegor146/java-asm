using System;
using System.Collections.Generic;
using System.Linq;
using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public enum VerificationElementType {
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

    public abstract class VerificationElement {
        public abstract VerificationElementType Type { get; }

        public abstract VerificationElement Copy();
    }

    public class SimpleVerificationElement : VerificationElement {
        public override VerificationElementType Type { get; }
        public override VerificationElement Copy() {
            return new SimpleVerificationElement(this.Type);
        }

        public SimpleVerificationElement(VerificationElementType type) {
            type.CheckInAndThrow(nameof(type), VerificationElementType.Top, VerificationElementType.Integer,
                VerificationElementType.Float, VerificationElementType.Long, VerificationElementType.Double, VerificationElementType.Null, VerificationElementType.UnitializedThis);
            this.Type = type;
        }
    }

    public class ObjectVerificationElement : VerificationElement {
        public override VerificationElementType Type => VerificationElementType.Object;
        public override VerificationElement Copy() {
            return new ObjectVerificationElement() {
                ObjectClass = this.ObjectClass?.Copy()
            };
        }

        public ClassName ObjectClass { get; set; }
    }

    public class UninitializedVerificationElement : VerificationElement {
        public override VerificationElementType Type => VerificationElementType.Unitialized;
        public override VerificationElement Copy() {
            return new UninitializedVerificationElement() {
                NewInstruction = (TypeInstruction) this.NewInstruction?.Copy()
            };
        }

        public TypeInstruction NewInstruction { get; set; }
    }

    public enum FrameType {
        Same,
        SameLocals1StackItem,
        Chop,
        Append,
        Full
    }

    public class StackMapFrame : Instruction {
        public override Opcode Opcode {
            get => Opcode.None;
            set => throw new InvalidOperationException(GetType().Name + " does not have an instruction");
        }

        public override Instruction Copy() {
            StackMapFrame insn = new StackMapFrame() {
                ChopK = this.ChopK
            };

            insn.Stack.AddRange(this.Stack.Select(a => a.Copy()).ToList());
            insn.Locals.AddRange(this.Stack.Select(a => a.Copy()).ToList());
            return insn;
        }

        public FrameType Type { get; set; }

        public List<VerificationElement> Stack { get; } = new List<VerificationElement>();

        public List<VerificationElement> Locals { get; } = new List<VerificationElement>();

        public byte? ChopK { get; set; }

        public override string ToString() {
            return "STACKFRAME";
        }
    }
}