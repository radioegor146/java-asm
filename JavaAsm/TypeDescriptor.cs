using System;
using System.Linq;
using System.Text;

namespace JavaAsm {
    /// <summary>
    /// Type descriptor
    /// </summary>
    public class TypeDescriptor : IDescriptor {
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
        public TypeDescriptor(ClassName className, int arrayDepth) {
            this.ArrayDepth = arrayDepth;
            this.ClassName = className ?? throw new ArgumentNullException(nameof(className));
        }

        /// <summary>
        /// Creates primitive type type descriptor
        /// </summary>
        /// <param name="primitiveType">Primitive type</param>
        /// <param name="arrayDepth">Array depth</param>
        public TypeDescriptor(PrimitiveType primitiveType, int arrayDepth) {
            this.ArrayDepth = arrayDepth;
            this.PrimitiveType = primitiveType;
        }

        /// <summary>
        /// Parses type descriptor from string
        /// </summary>
        /// <param name="descriptor">Source string to parse from</param>
        /// <param name="allowVoid">Allow void type</param>
        /// <returns>Parsed type descriptor</returns>
        public static TypeDescriptor Parse(string descriptor, bool allowVoid = false) {
            int offset = 0;
            TypeDescriptor parsedType = Parse(descriptor, ref offset, allowVoid);
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
        public static TypeDescriptor Parse(string descriptor, ref int offset, bool allowVoid = false) {
            int arrayDepth = 0;
            while (descriptor[offset] == '[') {
                arrayDepth++;
                offset++;
            }

            char typeChar = descriptor[offset];
            PrimitiveType primitiveType;
            switch (typeChar) {
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
                    StringBuilder className = new StringBuilder();
                    offset++;
                    while (descriptor[offset] != ';') {
                        className.Append(descriptor[offset]);
                        offset++;
                    }

                    offset++;
                    return new TypeDescriptor(new ClassName(className.ToString()), arrayDepth);
                default: throw new ArgumentOutOfRangeException(nameof(typeChar), $"Wrong type char: {typeChar}");
            }

            offset++;
            return new TypeDescriptor(primitiveType, arrayDepth);
        }

        /// <summary>
        /// Returns size of type on stack
        /// </summary>
        public int SizeOnStack => this.ArrayDepth == 0 && (this.PrimitiveType == JavaAsm.PrimitiveType.Double || this.PrimitiveType == JavaAsm.PrimitiveType.Long) ? 2 : this.PrimitiveType == JavaAsm.PrimitiveType.Void ? 0 : 1;

        /// <inheritdoc />
        public override string ToString() {
            string result = string.Join("", Enumerable.Repeat('[', this.ArrayDepth));
            if (this.PrimitiveType == null) {
                result += $"L{this.ClassName.Name};";
            }
            else {
                switch (this.PrimitiveType.Value) {
                    case global::JavaAsm.PrimitiveType.Boolean:   result += 'Z'; break;
                    case global::JavaAsm.PrimitiveType.Byte:      result += 'B'; break;
                    case global::JavaAsm.PrimitiveType.Character: result += 'C'; break;
                    case global::JavaAsm.PrimitiveType.Double:    result += 'D'; break;
                    case global::JavaAsm.PrimitiveType.Float:     result += 'F'; break;
                    case global::JavaAsm.PrimitiveType.Integer:   result += 'I'; break;
                    case global::JavaAsm.PrimitiveType.Long:      result += 'J'; break;
                    case global::JavaAsm.PrimitiveType.Short:     result += 'S'; break;
                    case global::JavaAsm.PrimitiveType.Void:      result += 'V'; break;
                    default: throw new ArgumentOutOfRangeException(nameof(this.PrimitiveType.Value));
                }
            }

            return result;
        }

        private bool Equals(TypeDescriptor other) {
            return this.ArrayDepth == other.ArrayDepth && Equals(this.ClassName, other.ClassName) && this.PrimitiveType == other.PrimitiveType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (obj is null)
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((TypeDescriptor) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                int hashCode = this.ArrayDepth;
                hashCode = (hashCode * 397) ^ (this.ClassName?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (this.PrimitiveType?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }

    /// <summary>
    /// Primitive types enum
    /// </summary>
    public enum PrimitiveType {
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