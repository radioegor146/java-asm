using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.Instructions;

namespace JavaDeobfuscator.JavaAsm.IO.ConstantPoolEntries
{
    internal class Frame : Instruction
    {
        public List<object> Locals { get; set; } = new List<object>();

        public List<object> Stack { get; set; } = new List<object>();

        public override Opcode Opcode => Opcode.None;
    }

    internal enum FramePrimitive
    {
        Top,
        Integer,
        Float,
        Long,
        Double,
        Null,
        UninitializedThis
    }
}
