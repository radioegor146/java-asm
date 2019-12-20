using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class LineNumber : Instruction
    {
        public override Opcode Opcode => Opcode.None;

        public ushort Line { get; set; }
    }
}
