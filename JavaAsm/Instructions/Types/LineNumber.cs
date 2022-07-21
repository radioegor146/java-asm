namespace JavaAsm.Instructions.Types
{
    public class LineNumber : Instruction
    {
        public override Opcode Opcode => Opcode.None;

        public ushort Line { get; set; }

        public override string ToString()
        {
            return $"LINE {this.Line}";
        }
    }
}
