namespace JavaAsm.Instructions.Types
{
    public class Label : Instruction
    {
        private static long globalLabelIndex;

        public long Index { get; }

        public override Opcode Opcode => Opcode.None;

        public Label()
        {
            this.Index = globalLabelIndex++;
        }

        public override string ToString()
        {
            return $"LABEL L{this.Index}";
        }
    }
}
