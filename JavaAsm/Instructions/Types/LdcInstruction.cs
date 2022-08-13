using System;
using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class LdcInstruction : Instruction {
        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), Opcode.LDC, Opcode.LDC_W, Opcode.LDC2_W);
        }

        public override Instruction Copy() {
            return new LdcInstruction() {
                Opcode = this.opcode,
                Value = CloneObject(this.Value)
            };
        }

        private static object CloneObject(object value) {
            if (value == null) {
                return null;
            }
            else if (value is int) {
                return (int) value;
            }
            else if (value is long) {
                return (long) value;
            }
            else if (value is float) {
                return (float) value;
            }
            else if (value is double) {
                return (double) value;
            }
            else if (value is string) {
                return (string) value;
            }
            else if (value is ClassName) {
                return ((ClassName) value).Copy();
            }
            else if (value is Handle) {
                return ((Handle) value).Copy();
            }
            else if (value is MethodDescriptor) {
                return ((MethodDescriptor) value).CopyMethodDescriptor();
            }
            else {
                throw new ArgumentException($"Unknown object type: {value.GetType().Name} -> {value}");
            }
        }

        public object Value { get; set; }

        public LdcInstruction() {
            this.Opcode = Opcode.LDC;
        }

        public LdcInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            string stringValue = this.Value.ToString();
            if (this.Value is string)
                stringValue = $"\"{stringValue}\"";
            return $"{this.Opcode} {stringValue}";
        }
    }
}