using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class LdcInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.LDC;

        public object Value { get; set; }

        public override string ToString()
        {
            var stringValue = Value.ToString();
            if (Value is string)
                stringValue = $"\"{stringValue}\"";
            return $"{Opcode} {stringValue}";
        }
    }
}
