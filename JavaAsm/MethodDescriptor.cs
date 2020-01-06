using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaAsm
{
    /// <summary>
    /// Descriptor of method. Consists of type descriptors of arguments and return value
    /// </summary>
    public class MethodDescriptor : IDescriptor
    {
        /// <summary>
        /// Return value type descriptor
        /// </summary>
        public TypeDescriptor ReturnType { get; }

        /// <summary>
        /// Arguments type descriptors
        /// </summary>
        public List<TypeDescriptor> ArgumentTypes { get; } 

        /// <summary>
        /// Creates method descriptor
        /// </summary>
        /// <param name="returnType">Return type</param>
        /// <param name="argumentTypes">Argument types</param>
        public MethodDescriptor(TypeDescriptor returnType, params TypeDescriptor[] argumentTypes) : this(returnType, argumentTypes.ToList()) { }
        
        /// <summary>
        /// Creates method descriptor
        /// </summary>
        /// <param name="returnType">Return type</param>
        /// <param name="argumentTypes">Argument types</param>
        public MethodDescriptor(TypeDescriptor returnType, List<TypeDescriptor> argumentTypes)
        {
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ArgumentTypes = argumentTypes ?? throw new ArgumentNullException(nameof(argumentTypes));
        }

        /// <summary>
        /// Parses method descriptor from string
        /// </summary>
        /// <param name="descriptor">Source string</param>
        /// <returns>Parsed method descriptor</returns>
        public static MethodDescriptor Parse(string descriptor)
        {
            var offset = 0;
            if (descriptor[offset] != '(')
                throw new FormatException($"Wrong method descriptor: {descriptor}");
            offset++;
            var argumentTypes = new List<TypeDescriptor>();
            while (descriptor[offset] != ')')
                argumentTypes.Add(TypeDescriptor.Parse(descriptor, ref offset));
            offset++;
            return new MethodDescriptor(TypeDescriptor.Parse(descriptor, ref offset, true), argumentTypes);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append('(');
            foreach (var argumentType in ArgumentTypes)
                result.Append(argumentType);
            result.Append(')');
            result.Append(ReturnType);
            return result.ToString();
        }

        private bool Equals(MethodDescriptor other)
        {
            return Equals(ReturnType, other.ReturnType) && Equals(ArgumentTypes, other.ArgumentTypes);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MethodDescriptor) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (ReturnType.GetHashCode() * 397) ^ ArgumentTypes.GetHashCode();
            }
        }
    }
}