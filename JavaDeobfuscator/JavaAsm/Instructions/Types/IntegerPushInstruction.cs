using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class IntegerPushInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ushort Value { get; set; }

        public IntegerPushInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.BIPUSH, Opcode.SIPUSH);
            Opcode = opcode;
        }
    }
}
