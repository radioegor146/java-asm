using System;

namespace JavaAsm.Instructions.Types {
    public class Label : Instruction {
        public static long GlobalLabelIndex { get; set; }

        /// <summary>
        /// A readable index. Modifying this has no purpose, but it can be useful for visualizing labels
        /// </summary>
        public long Index { get; set; }

        public override Opcode Opcode {
            get => Opcode.None;
            set => throw new InvalidOperationException(GetType().Name + " does not have an instruction");
        }

        public override Instruction Copy() {
            return new Label() {
                Index = this.Index
            };
        }

        public Label() {
            this.Index = GlobalLabelIndex++;
        }

        public override string ToString() {
            return $"LABEL L{this.Index}";
        }
    }
}