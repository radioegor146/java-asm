using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types
{
    public class JumpInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public Label Target { get; set; }
            
        public JumpInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.IFEQ, Opcode.IFNE, Opcode.IFLT, Opcode.IFGE, Opcode.IFGT,
                Opcode.IFLE, Opcode.IF_ICMPEQ, Opcode.IF_ICMPNE, Opcode.IF_ICMPLT, Opcode.IF_ICMPGE, Opcode.IF_ICMPGT,
                Opcode.IF_ICMPLE, Opcode.IF_ACMPEQ, Opcode.IF_ACMPNE, Opcode.GOTO, Opcode.JSR, Opcode.IFNULL, Opcode.IFNONNULL);
            Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{Opcode} L{Target.Index}";
        }
    }
}
