using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class IncrementInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.IINC;

        public ushort VariableIndex { get; set; }

        public ushort Value { get; set; }

        public override string ToString()
        {
            return $"{Opcode} {VariableIndex} {Value}";
        }
    }
}
