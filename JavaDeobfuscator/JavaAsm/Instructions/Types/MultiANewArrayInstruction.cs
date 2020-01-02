using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class MultiANewArrayInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.MULTIANEWARRAY;

        public ClassName Type { get; set; }

        public byte Dimensions { get; set; }

        public override string ToString()
        {
            return $"{Opcode} {Type} {Dimensions}";
        }
    }
}
