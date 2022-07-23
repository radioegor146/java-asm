using System;
using System.Collections.Generic;

namespace JavaAsm.Instructions.Types {
    public class TableSwitchInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.TABLESWITCH;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 opcode");
        }

        public Label Default { get; set; }

        public int LowValue { get; set; }

        public int HighValue { get; set; }

        public List<Label> Labels { get; set; } = new List<Label>();
    }
}