using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class LdcInsnNode : Instruction
    {
        public override Opcode Opcode => Opcode.LDC;

        public object Value { get; set; }
    }
}
