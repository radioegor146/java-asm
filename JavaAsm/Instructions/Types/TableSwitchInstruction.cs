using System;
using System.Collections.Generic;

namespace JavaAsm.Instructions.Types {
    public class TableSwitchInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.TABLESWITCH;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 opcode");
        }

        public override Instruction Copy() {
            return new TableSwitchInstruction() {
                Default = this.Default,
                LowValue = this.LowValue,
                HighValue = this.HighValue,
                Labels = new List<Label>(this.Labels)
            };
        }

        public Label Default { get; set; }

        public int LowValue { get; set; }

        public int HighValue { get; set; }

        public List<Label> Labels { get; set; } = new List<Label>();
    }
}