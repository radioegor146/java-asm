using System;

namespace JavaAsm.Instructions.Types {
    public class NewArrayInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.NEWARRAY;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 instruction");
        }

        public override Instruction Copy() {
            return new NewArrayInstruction() {
                ArrayType = this.ArrayType
            };
        }

        public NewArrayTypeCode ArrayType { get; set; }

        public override string ToString() {
            return $"{this.Opcode} {this.ArrayType}";
        }
    }

    public enum NewArrayTypeCode : byte {
        Boolean = 4,
        Character,
        Float,
        Double,
        Byte,
        Short,
        Integer,
        Long
    }
}