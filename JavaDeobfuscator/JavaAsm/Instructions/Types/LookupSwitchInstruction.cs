using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class LookupSwitchInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.LOOKUPSWITCH;

        public Label Default { get; set; }

        public List<KeyValuePair<int, Label>> MatchLabels { get; set; } = new List<KeyValuePair<int, Label>>();
    }
}
