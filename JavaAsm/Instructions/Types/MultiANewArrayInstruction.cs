using System;

namespace JavaAsm.Instructions.Types {
    public class MultiANewArrayInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.MULTIANEWARRAY;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 instruction");
        }

        public ClassName Type { get; set; }

        public byte Dimensions { get; set; }

        public override string ToString() {
            return $"{this.Opcode} {this.Type} {this.Dimensions}";
        }
    }
}