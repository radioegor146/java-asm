using System;
using System.Linq;

namespace JavaAsm {
    [Flags]
    public enum ClassAccessModifiers : ushort {
        Public = 0x0001,
        Protected = 0x0004,
        Private = 0x0002,
        Static = 0x0008,
        Abstract = 0x0400,
        Final = 0x0010,
        Strict = 0x0800,
        Annotation = 0x2000,
        Enum = 0x4000,
        Interface = 0x0200,
        Super = 0x0020,
        Synthetic = 0x1000
    }

    [Flags]
    public enum MethodAccessModifiers : ushort {
        Public = 0x0001,
        Protected = 0x0004,
        Private = 0x0002,
        Static = 0x0008,
        Abstract = 0x0400,
        Syncrionized = 0x0020,
        Final = 0x0010,
        Native = 0x0100,
        Strict = 0x0800,
        Bridge = 0x0040,
        Synthetic = 0x1000,
        Varargs = 0x0080
    }

    [Flags]
    public enum FieldAccessModifiers : ushort {
        Public = 0x0001,
        Protected = 0x0004,
        Private = 0x0002,
        Static = 0x0008,
        Transient = 0x0080,
        Volatile = 0x0040,
        Final = 0x0010,
        Synthetic = 0x1000
    }

    public static class AccessModifiersExtensions {
        public static string ToString(ClassAccessModifiers accessModifiers) => string.Join(" ", Enum.GetValues(typeof(ClassAccessModifiers)).OfType<ClassAccessModifiers>().Where(x => accessModifiers.HasFlag(x)).Select(x => x.ToString().ToLower()));

        public static string ToString(MethodAccessModifiers accessModifiers) => string.Join(" ", Enum.GetValues(typeof(MethodAccessModifiers)).OfType<MethodAccessModifiers>().Where(x => accessModifiers.HasFlag(x)).Select(x => x.ToString().ToLower()));

        public static string ToString(FieldAccessModifiers accessModifiers) => string.Join(" ", Enum.GetValues(typeof(FieldAccessModifiers)).OfType<FieldAccessModifiers>().Where(x => accessModifiers.HasFlag(x)).Select(x => x.ToString().ToLower()));
    }
}