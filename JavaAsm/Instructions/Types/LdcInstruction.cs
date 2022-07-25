using System;
using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class LdcInstruction : Instruction {
        private Opcode opcode;
        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), Opcode.LDC, Opcode.LDC_W, Opcode.LDC2_W);
        }

        public object Value { get; set; }

        public LdcInstruction() {
            this.Opcode = Opcode.LDC;
        }

        public override string ToString() {
            string stringValue = this.Value.ToString();
            if (this.Value is string)
                stringValue = $"\"{stringValue}\"";
            return $"{this.Opcode} {stringValue}";
        }
    }
}