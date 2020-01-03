using System;
using System.Collections.Generic;
using System.Text;
using JavaDeobfuscator.JavaAsm.Helpers;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class StackMapTableAttribute
    {
        public enum StackElementType
        {
            Top,
            Integer,
            Float,
            Long,
            Double,
            Null,
            UnitializedThis,
            Object,
            Unitialized
        }

        public abstract class StackElement
        {
            public abstract StackElementType Type { get; }
        }

        public class SimpleStackElement : StackElement
        {
            public override StackElementType Type { get; }

            public SimpleStackElement(StackElementType type)
            {
                type.CheckInAndThrow(nameof(type), StackElementType.Top, StackElementType.Integer,
                    StackElementType.Float, StackElementType.Long, StackElementType.Double, StackElementType.Null, StackElementType.UnitializedThis);
                Type = type;
            }
        }

        public class ObjectStackElement : StackElement
        {
            public override StackElementType Type => StackElementType.Object;

            public ClassName ObjectClass { get; set; }
        }

        public class UninitializedStackElement : StackElement
        {
            public override StackElementType Type => StackElementType.Unitialized;

            public ushort NewInstructionOffset { get; set; }
        }

        public enum FrameType
        {
            Same,
            SameLocals1StackItem,
            SameLocals1StackItemExtended,
            Chop,
            SameExtended,
            Append,
            Full
        }

        public class StackMapFrame
        {
            public FrameType Type { get; set; }
        }
    }
}
