using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class VariableInstruction : Instruction {
        private static readonly Opcode[] ALLOW_OPCODES = new Opcode[] {
            Opcode.ILOAD, Opcode.ILOAD_0, Opcode.ILOAD_1, Opcode.ILOAD_2, Opcode.ILOAD_3,
            Opcode.LLOAD, Opcode.LLOAD_0, Opcode.LLOAD_1, Opcode.LLOAD_2, Opcode.LLOAD_3,
            Opcode.FLOAD, Opcode.FLOAD_0, Opcode.FLOAD_1, Opcode.FLOAD_2, Opcode.FLOAD_3,
            Opcode.DLOAD, Opcode.DLOAD_0, Opcode.DLOAD_1, Opcode.DLOAD_2, Opcode.DLOAD_3,
            Opcode.ALOAD, Opcode.ALOAD_0, Opcode.ALOAD_1, Opcode.ALOAD_2, Opcode.ALOAD_3,
            Opcode.ISTORE, Opcode.ISTORE_0, Opcode.ISTORE_1, Opcode.ISTORE_2, Opcode.ISTORE_3,
            Opcode.LSTORE, Opcode.LSTORE_0, Opcode.LSTORE_1, Opcode.LSTORE_2, Opcode.LSTORE_3,
            Opcode.FSTORE, Opcode.FSTORE_0, Opcode.FSTORE_1, Opcode.FSTORE_2, Opcode.FSTORE_3,
            Opcode.DSTORE, Opcode.DSTORE_0, Opcode.DSTORE_1, Opcode.DSTORE_2, Opcode.DSTORE_3,
            Opcode.ASTORE, Opcode.ASTORE_0, Opcode.ASTORE_1, Opcode.ASTORE_2, Opcode.ASTORE_3,
            Opcode.RET
        };

        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), ALLOW_OPCODES);
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