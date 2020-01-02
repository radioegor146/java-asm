using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class MethodInstruction : Instruction
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