namespace JavaAsm.Instructions.Types
{
    public class MultiANewArrayInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.MULTIANEWARRAY;

        public ClassName Type { get; set; }

        public byte Dimensions { get; set; }

        public override string ToString()
        {
            return $"{this.Opcode} {this.Type} {this.Dimensions}";
        }
    }
}
