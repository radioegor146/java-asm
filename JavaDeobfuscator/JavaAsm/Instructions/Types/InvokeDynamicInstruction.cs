using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions.Types
{
    internal class InvokeDynamicInstruction : Instruction
    {
        public override Opcode Opcode => Opcode.INVOKEDYNAMIC;

        public string Name { get; set; }

        public MethodDescriptor Descriptor { get; set; }

        public Handle BootstrapMethod { get; set; }

        public List<object> BootstrapMethodArgs { get; set; }
    }

    internal class Handle
    {
        public ClassName Owner { get; set; }

        public string Name { get; set; }

        public IDescriptor Descriptor { get; set; }
    }
}
