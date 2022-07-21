using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types
{
    public class FieldInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public TypeDescriptor Descriptor { get; set; }

        public FieldInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.GETFIELD, Opcode.GETSTATIC, Opcode.PUTFIELD,
                Opcode.PUTSTATIC);
            this.Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{this.Opcode} {this.Owner}.{this.Name} {this.Descriptor}";
        }
    }
}
