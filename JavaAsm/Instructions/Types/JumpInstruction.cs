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

        public Label Target { get; set; }

        /// <summary>
        /// Use a manual branch offset, instead of a calculated offset
        /// <para>
        /// <see cref="JumpOffset"/> will not be checked or verified
        /// </para>
        /// </summary>
        public bool UseOverrideOffset { get; set; }

        public int JumpOffset { get; set; }

        public JumpInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} L{this.Target.Index}";
        }
    }
}