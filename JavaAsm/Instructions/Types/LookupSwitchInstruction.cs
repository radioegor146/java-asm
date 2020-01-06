using System.Collections.Generic;

namespace JavaAsm.Instructions.Types
{
    public class LookupSwitchInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.LOOKUPSWITCH;

        public Label Default { get; set; }

        public List<KeyValuePair<int, Label>> MatchLabels { get; set; } = new List<KeyValuePair<int, Label>>();
    }
}
