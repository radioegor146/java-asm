using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class MultiANewArrayInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.MULTIANEWARRAY;

        public TypeDescriptor Descriptor { get; set; }

        public byte Dimensions { get; set; }
    }
}
