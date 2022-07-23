using System;

namespace JavaAsm.Instructions.Types {
    public class IncrementInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.IINC;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 opcode");
        }

        public ushort VariableIndex { get; set; }

        public short Value { get; set; }

        public override string ToString() {
            return $"{this.Opcode} {this.VariableIndex} {this.Value}";
        }
    }
}