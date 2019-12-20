using System;
using System.Linq;
using System.Text;

namespace JavaDeobfuscator.JavaAsm
{
    internal class TypeDescriptor : IDescriptor
    {
        public ClassName ClassName { get; }

        public PrimitiveType? PrimitiveType { get; }

        public int ArrayDepth { get; }

        public TypeDescriptor(ClassName className, int arrayDepth)
        {
            ArrayDepth = arrayDepth;
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
        }

        public TypeDescriptor(PrimitiveType primitiveType, int arrayDepth)
        {
            ArrayDepth = arrayDepth;
            PrimitiveType = primitiveType;
        }

        public static TypeDescriptor Parse(string descriptor, bool allowVoid = false)
        {
            var offset = 0;
            var parsedType = Parse(descriptor, ref offset, allowVoid);
            if (offset != descriptor.Length)
                throw new FormatException($"Exceed charaters in descriptor: {descriptor}");
            return parsedType;
        }

        public static TypeDescriptor Parse(string descriptor, ref int offset, bool allowVoid = false)
        {
            var arrayDepth = 0;
            while (descriptor[offset] == '[')
            {
                arrayDepth++;
                offset++;
            }

            var typeChar = descriptor[offset];
            PrimitiveType primitiveType;
            switch (typeChar)
            {
                case 'B':
                    primitiveType = JavaAsm.PrimitiveType.Byte;
                    break;
                case 'C':
                    primitiveType = JavaAsm.PrimitiveType.Character;
                    break;
                case 'D':
                    primitiveType = JavaAsm.PrimitiveType.Double;
                    break;
                case 'F':
                    primitiveType = JavaAsm.PrimitiveType.Float;
                    break;
                case 'I':
                    primitiveType = JavaAsm.PrimitiveType.Integer;
                    break;
                case 'J':
                    primitiveType = JavaAsm.PrimitiveType.Long;
                    break;
                case 'S':
                    primitiveType = JavaAsm.PrimitiveType.Short;
                    break;
                case 'Z':
                    primitiveType = JavaAsm.PrimitiveType.Boolean;
                    break;
                case 'V':
                    if (!allowVoid)
                        throw new FormatException("Void is not allowed");
                    primitiveType = JavaAsm.PrimitiveType.Void;
                    break;
                case 'L':
                    var className = new StringBuilder();
                    offset++;
                    while (descriptor[offset] != ';')
                    {
                        className.Append(descriptor[offset]);
                        offset++;
                    }

                    offset++;
                    return new TypeDescriptor(new ClassName(className.ToString()), arrayDepth);
                default:
                    throw new ArgumentException($"Wrong type char: {typeChar}");
            }

            offset++;
            return new TypeDescriptor(primitiveType, arrayDepth);
        }

        public override string ToString()
        {
            var result = string.Join("", Enumerable.Repeat('[', ArrayDepth));
            if (PrimitiveType == null)
            {
                result += $"L{ClassName.Name};";
            }
            else
            {
                result += PrimitiveType.Value switch
                {
                    JavaAsm.PrimitiveType.Boolean => 'Z',
                    JavaAsm.PrimitiveType.Byte => 'B',
                    JavaAsm.PrimitiveType.Character => 'C',
                    JavaAsm.PrimitiveType.Double => 'D',
                    JavaAsm.PrimitiveType.Float => 'F',
                    JavaAsm.PrimitiveType.Integer => 'I',
                    JavaAsm.PrimitiveType.Long => 'J',
                    JavaAsm.PrimitiveType.Short => 'S',
                    JavaAsm.PrimitiveType.Void => 'V',
                    _ => throw new ArgumentOutOfRangeException(nameof(PrimitiveType.Value))
                };
            }
            return result;
        }

        protected bool Equals(TypeDescriptor other)
        {
            return ArrayDepth == other.ArrayDepth && Equals(ClassName, other.ClassName) && PrimitiveType == other.PrimitiveType;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TypeDescriptor) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ArrayDepth;
                hashCode = (hashCode * 397) ^ (ClassName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PrimitiveType?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }

    internal enum PrimitiveType
    {
        Boolean,
        Byte,
        Character,
        Double,
        Float,
        Integer,
        Long,
        Short,
        Void
    }
}