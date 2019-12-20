using System;

namespace JavaDeobfuscator.JavaAsm
{
    [Flags]
    internal enum AccessModifiers : ushort
    {
        Abstract = 0x0400,
        Annotation = 0x2000,
        Bridge = 0x0040,
        Enum = 0x4000,
        Final = 0x0010,
        Interface = 0x0200,
        Mandated = 0x8000,
        Native = 0x0100,
        Private = 0x0002,
        Protected = 0x0004,
        Public = 0x0001,
        Static = 0x0008,
        Strict = 0x0800,
        Super = 0x0020,
        Syncrionized = 0x0020,
        Synthetic = 0x1000,
        Transient = 0x0080,
        Varargs = 0x0080,
        Volatile = 0x0040
    }
}