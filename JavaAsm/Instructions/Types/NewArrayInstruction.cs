namespace JavaAsm.Instructions.Types
{
    public class NewArrayInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.NEWARRAY;

        public NewArrayTypeCode ArrayType { get; set; }

        public override string ToString()
        {
            return $"{this.Opcode} {this.ArrayType}";
        }
    }

    public enum NewArrayTypeCode : byte
    {
        Boolean = 4,
        Character,
        Float,
        Double,
        Byte,
        Short,
        Integer,
        Long
    }
}
