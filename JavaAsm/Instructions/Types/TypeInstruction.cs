using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class TypeInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), Opcode.NEW, Opcode.ANEWARRAY, Opcode.CHECKCAST, Opcode.INSTANCEOF);
        }

        public ClassName Type { get; set; }

        public TypeInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} {this.Type}";
        }
    }
}