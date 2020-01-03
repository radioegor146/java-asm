using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class NewArrayInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.NEWARRAY;

        public NewArrayTypeCode ArrayType { get; set; }

        public override string ToString()
        {
            return $"{Opcode} {ArrayType}";
        }
    }

    internal enum NewArrayTypeCode : byte
    {
        Boolean = 4,
        Character,
        Float,
        Double,
        Byte,
        Short,
        Integer,
        Long
    }
}
