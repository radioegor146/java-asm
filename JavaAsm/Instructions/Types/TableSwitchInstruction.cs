using System.Collections.Generic;

namespace JavaAsm.Instructions.Types
{
    public class TableSwitchInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.TABLESWITCH;

        public Label Default { get; set; }

        public int LowValue { get; set; }

        public int HighValue { get; set; }

        public List<Label> Labels { get; set; } = new List<Label>();
    }
}
