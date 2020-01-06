using System;
using System.Linq;
using System.Text;

namespace JavaAsm
{
    /// <summary>
    /// Type descriptor
    /// </summary>
    public class TypeDescriptor : IDescriptor
    {
        /// <summary>
        /// Class name. Equals to null if type is primitive
        /// </summary>
        public ClassName ClassName { get; }

        /// <summary>
        /// Primitive type. Equals to null if type is object-based
        /// </summary>
        public PrimitiveType? PrimitiveType { get; }

        /// <summary>
        /// Depth of array or zero if it is not an array
        /// </summary>
        public int ArrayDepth { get; }

        /// <summary>
        /// Creates class type descriptor
        /// </summary>
        /// <param name="className">Class</param>
        /// <param name="arrayDepth">Array depth</param>
        public TypeDescriptor(ClassName className, int arrayDepth)
        {
            ArrayDepth = arrayDepth;
            ClassName = className ?? throw new ArgumentNullException(nameof(className));
        }

        /// <summary>
        /// Creates primitive type type descriptor
        /// </summary>
        /// <param name="primitiveType">Primitive type</param>
        /// <param name="arrayDepth">Array depth</param>
        public TypeDescriptor(PrimitiveType primitiveType, int arrayDepth)
        {
            ArrayDepth = arrayDepth;
            PrimitiveType = primitiveType;
        }

        /// <summary>
        /// Parses type descriptor from string
        /// </summary>
        /// <param name="descriptor">Source string to parse from</param>
        /// <param name="allowVoid">Allow void type</param>
        /// <returns>Parsed type descriptor</returns>
        public static TypeDescriptor Parse(string descriptor, bool allowVoid = false)
        {
            var offset = 0;
            var parsedType = Parse(descriptor, ref offset, allowVoid);
            if (offset != descriptor.Length)
                throw new FormatException($"Exceed charaters in descriptor: {descriptor}");
            return parsedType;
        }

        /// <summary>
        /// Parses type descriptor from string starting from specified offset
        /// </summary>
        /// <param name="descriptor">Source string to parse from</param>
        /// <param name="offset">Offset in source string to parse from</param>
        /// <param name="allowVoid">Allow void type</param>
        /// <returns>Parsed type descriptor</returns>
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
                    primitiveType = global::JavaAsm.PrimitiveType.Byte;
                    break;
                case 'C':
                    primitiveType = global::JavaAsm.PrimitiveType.Character;
                    break;
                case 'D':
                    primitiveType = global::JavaAsm.PrimitiveType.Double;
                    break;
                case 'F':
                    primitiveType = global::JavaAsm.PrimitiveType.Float;
                    break;
                case 'I':
                    primitiveType = global::JavaAsm.PrimitiveType.Integer;
                    break;
                case 'J':
                    primitiveType = global::JavaAsm.PrimitiveType.Long;
                    break;
                case 'S':
                    primitiveType = global::JavaAsm.PrimitiveType.Short;
                    break;
                case 'Z':
                    primitiveType = global::JavaAsm.PrimitiveType.Boolean;
                    break;
                case 'V':
                    if (!allowVoid)
                        throw new FormatException("Void is not allowed");
                    primitiveType = global::JavaAsm.PrimitiveType.Void;
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
                    throw new ArgumentOutOfRangeException(nameof(typeChar), $"Wrong type char: {typeChar}");
            }

            offset++;
            return new TypeDescriptor(primitiveType, arrayDepth);
        }

        /// <summary>
        /// Returns size of type on stack
        /// </summary>
        public int SizeOnStack => ArrayDepth == 0 && (PrimitiveType == JavaAsm.PrimitiveType.Double || PrimitiveType == JavaAsm.PrimitiveType.Long) ? 2 : 
            PrimitiveType == JavaAsm.PrimitiveType.Void ? 0 : 1;

        /// <inheritdoc />
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
                    global::JavaAsm.PrimitiveType.Boolean => 'Z',
                    global::JavaAsm.PrimitiveType.Byte => 'B',
                    global::JavaAsm.PrimitiveType.Character => 'C',
                    global::JavaAsm.PrimitiveType.Double => 'D',
                    global::JavaAsm.PrimitiveType.Float => 'F',
                    global::JavaAsm.PrimitiveType.Integer => 'I',
                    global::JavaAsm.PrimitiveType.Long => 'J',
                    global::JavaAsm.PrimitiveType.Short => 'S',
                    global::JavaAsm.PrimitiveType.Void => 'V',
                    _ => throw new ArgumentOutOfRangeException(nameof(PrimitiveType.Value))
                };
            }
            return result;
        }

        private bool Equals(TypeDescriptor other)
        {
            return ArrayDepth == other.ArrayDepth && Equals(ClassName, other.ClassName) && PrimitiveType == other.PrimitiveType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TypeDescriptor) obj);
        }

        /// <inheritdoc />
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

    /// <summary>
    /// Primitive types enum
    /// </summary>
    public enum PrimitiveType
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