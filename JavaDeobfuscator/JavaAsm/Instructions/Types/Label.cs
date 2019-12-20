using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class Label : Instruction
    {
        public override Opcode Opcode => Opcode.None;
    }
}
