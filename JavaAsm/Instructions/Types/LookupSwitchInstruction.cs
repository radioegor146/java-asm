using System;
using System.Collections.Generic;
using System.Linq;

namespace JavaAsm.Instructions.Types {
    public class LookupSwitchInstruction : Instruction {
        public override Opcode Opcode {
            get => Opcode.LOOKUPSWITCH;
            set => throw new InvalidOperationException(GetType().Name + " only has 1 instruction");
        }

        public override Instruction Copy() {
            return new LookupSwitchInstruction() {
                Default = this.Default,
                MatchLabels = new List<KeyValuePair<int, Label>>(this.MatchLabels.Select(a => new KeyValuePair<int, Label>(a.Key, a.Value)))
            };
        }

        public Label Default { get; set; }

        public List<KeyValuePair<int, Label>> MatchLabels { get; set; } = new List<KeyValuePair<int, Label>>();
    }
}