using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class IntegerPushInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), Opcode.BIPUSH, Opcode.SIPUSH);
        }

        public override Instruction Copy() {
            return new IntegerPushInstruction(this.opcode) {
                Value = this.Value
            };
        }

        public ushort Value { get; set; }

        public IntegerPushInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} {this.Value}";
        }
    }
}