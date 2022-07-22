using System;

namespace JavaAsm.Instructions.Types {
    public class Label : Instruction {
        private static long globalLabelIndex;

        public long Index { get; }

        public override Opcode Opcode {
            get => Opcode.None;
            set => throw new InvalidOperationException(GetType().Name + " does not have an instruction");
        }

        public Label() {
            this.Index = globalLabelIndex++;
        }

        public override string ToString() {
            return $"LABEL L{this.Index}";
        }
    }
}