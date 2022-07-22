using System;
using System.Collections.Generic;

namespace JavaAsm.Instructions.Types {
    public class LookupSwitchInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.LOOKUPSWITCH;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 instruction");
        }

        public Label Default { get; set; }

        public List<KeyValuePair<int, Label>> MatchLabels { get; set; } = new List<KeyValuePair<int, Label>>();
    }
}