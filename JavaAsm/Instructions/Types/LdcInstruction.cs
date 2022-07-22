using System;

namespace JavaAsm.Instructions.Types {
    public class LdcInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.LDC;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 instruction");
        }

        public object Value { get; set; }

        public override string ToString() {
            string stringValue = this.Value.ToString();
            if (this.Value is string)
                stringValue = $"\"{stringValue}\"";
            return $"{this.Opcode} {stringValue}";
        }
    }
}