using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types
{
    public class IntegerPushInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ushort Value { get; set; }

        public IntegerPushInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.BIPUSH, Opcode.SIPUSH);
            this.Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{this.Opcode} {this.Value}";
        }
    }
}
