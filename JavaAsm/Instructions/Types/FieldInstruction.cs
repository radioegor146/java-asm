using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class FieldInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), Opcode.GETFIELD, Opcode.GETSTATIC, Opcode.PUTFIELD, Opcode.PUTSTATIC);
        }

        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public TypeDescriptor Descriptor { get; set; }

        public FieldInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode} {this.Owner}.{this.Name} {this.Descriptor}";
        }
    }
}