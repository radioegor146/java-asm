using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions
{
    internal abstract class Instruction : ICloneable
    {
        public InstructionList OwnerList { get; internal set; }

        public Instruction Previous { get; internal set; }

        public Instruction Next { get; internal set; }

        public abstract Opcode Opcode { get; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
