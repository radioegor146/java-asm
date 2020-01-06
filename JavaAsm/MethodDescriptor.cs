using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JavaAsm
{
    public class MethodDescriptor : IDescriptor
    {
        public TypeDescriptor ReturnType { get; }

        public List<TypeDescriptor> ArgumentsTypes { get; } 

        public MethodDescriptor(TypeDescriptor returnType, params TypeDescriptor[] argumentTypes) : this(returnType, argumentTypes.ToList()) { }

        public MethodDescriptor(TypeDescriptor returnType, List<TypeDescriptor> argumentsTypes)
        {
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ArgumentsTypes = argumentsTypes ?? throw new ArgumentNullException(nameof(argumentsTypes));
        }

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

        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append('(');
            foreach (var argumentType in ArgumentsTypes)
                result.Append(argumentType);
            result.Append(')');
            result.Append(ReturnType);
            return result.ToString();
        }

        public bool Equals(MethodDescriptor other)
        {
            return Equals(ReturnType, other.ReturnType) && Equals(ArgumentsTypes, other.ArgumentsTypes);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MethodDescriptor) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ReturnType.GetHashCode() * 397) ^ ArgumentsTypes.GetHashCode();
            }
        }
    }
}