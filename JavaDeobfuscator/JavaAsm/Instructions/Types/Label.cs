using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class Label : Instruction
    {
        private static long globalLabelIndex;

        public long Index { get; }

        public override Opcode Opcode => Opcode.None;

        public Label()
        {
            Index = globalLabelIndex++;
        }

        public override string ToString()
        {
            return $"LABEL L{Index}";
        }
    }
}
