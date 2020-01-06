namespace JavaAsm.Instructions.Types
{
    public class LdcInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.LDC;

        public object Value { get; set; }

        public override string ToString()
        {
            var stringValue = Value.ToString();
            if (Value is string)
                stringValue = $"\"{stringValue}\"";
            return $"{Opcode} {stringValue}";
        }
    }
}
