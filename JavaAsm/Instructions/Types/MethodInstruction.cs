using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class MethodInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), Opcode.INVOKESTATIC, Opcode.INVOKEVIRTUAL, Opcode.INVOKEINTERFACE, Opcode.INVOKESPECIAL);
        }

        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public MethodDescriptor Descriptor { get; set; }

        public MethodInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} {this.Owner}.{this.Name}{this.Descriptor}";
        }
    }
}