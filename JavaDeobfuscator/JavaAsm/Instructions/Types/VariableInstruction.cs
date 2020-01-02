using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class VariableInstruction : Instruction
    {
        public override Opcode Opcode { get; }

        public ushort VariableIndex { get; set; }

        public VariableInstruction(Opcode opcode)
        {
            opcode.CheckInAndThrow(nameof(opcode), Opcode.ILOAD, Opcode.LLOAD, Opcode.FLOAD, Opcode.DLOAD, Opcode.ALOAD,
                Opcode.ISTORE, Opcode.LSTORE, Opcode.FSTORE, Opcode.DSTORE, Opcode.ASTORE, Opcode.RET);
            Opcode = opcode;
        }

        public override string ToString()
        {
            return $"{Opcode} {VariableIndex}";
        }
    }
}
