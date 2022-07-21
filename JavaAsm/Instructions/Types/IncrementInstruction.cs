namespace JavaAsm.Instructions.Types
{
    public class IncrementInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.IINC;

        public ushort VariableIndex { get; set; }

        public short Value { get; set; }

        public override string ToString()
        {
            return $"{this.Opcode} {this.VariableIndex} {this.Value}";
        }
    }
}
