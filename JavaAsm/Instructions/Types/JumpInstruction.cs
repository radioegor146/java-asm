using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class JumpInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value),
                Opcode.IFEQ, Opcode.IFNE, Opcode.IFLT, Opcode.IFGE, Opcode.IFGT, Opcode.IFLE,
                Opcode.IF_ICMPEQ, Opcode.IF_ICMPNE, Opcode.IF_ICMPLT, Opcode.IF_ICMPGE,
                Opcode.IF_ICMPGT, Opcode.IF_ICMPLE, Opcode.IF_ACMPEQ, Opcode.IF_ACMPNE,
                Opcode.GOTO, Opcode.JSR, Opcode.IFNULL, Opcode.IFNONNULL);
        }

        public override Instruction Copy() {
            return new JumpInstruction(this.opcode) {
                JumpOffset = this.JumpOffset,
                Target = this.Target,
            };
        }

        public Label Target { get; set; }

        public int JumpOffset { get; set; }

        public JumpInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} L{(this.Target == null ? "[No target]" : this.Target.Index.ToString())}";
        }
    }
}