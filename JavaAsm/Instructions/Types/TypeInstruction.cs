using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types
{
    public class TypeInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ClassName Type { get; set; }

        public TypeInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.NEW, Opcode.ANEWARRAY, Opcode.CHECKCAST, Opcode.INSTANCEOF);
            this.Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{this.Opcode} {this.Type}";
        }
    }
}
