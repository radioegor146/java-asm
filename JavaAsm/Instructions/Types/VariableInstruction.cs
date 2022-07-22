using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class VariableInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value),
                Opcode.ILOAD, Opcode.LLOAD, Opcode.FLOAD, Opcode.DLOAD,
                Opcode.ALOAD, Opcode.ISTORE, Opcode.LSTORE, Opcode.FSTORE,
                Opcode.DSTORE, Opcode.ASTORE, Opcode.RET);
        }

        public ushort VariableIndex { get; set; }

        public VariableInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} {this.VariableIndex}";
        }
    }
}
