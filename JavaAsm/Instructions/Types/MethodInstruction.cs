using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types
{
    public class MethodInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public MethodDescriptor Descriptor { get; set; }

        public MethodInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.INVOKESTATIC, Opcode.INVOKEVIRTUAL, Opcode.INVOKEINTERFACE,
                Opcode.INVOKESPECIAL);
            Opcode = opcode;
        }
        public override string ToString()
        {
            return $"{Opcode} {Owner}.{Name}{Descriptor}";
        }
    }
}