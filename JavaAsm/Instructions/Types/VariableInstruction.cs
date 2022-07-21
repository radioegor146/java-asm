using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types
{
    public class VariableInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ushort VariableIndex { get; set; }

        public VariableInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.ILOAD, Opcode.LLOAD, Opcode.FLOAD, Opcode.DLOAD, Opcode.ALOAD,
                Opcode.ISTORE, Opcode.LSTORE, Opcode.FSTORE, Opcode.DSTORE, Opcode.ASTORE, Opcode.RET);
            this.Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{this.Opcode} {this.VariableIndex}";
        }
    }
}
