using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class FieldInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public TypeDescriptor Descriptor { get; set; }

        public FieldInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.GETFIELD, Opcode.GETSTATIC, Opcode.PUTFIELD,
                Opcode.PUTSTATIC);
            Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{Opcode} {Owner}.{Name} {Descriptor}";
        }
    }
}
