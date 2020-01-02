using System;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm
{
    internal class ClassName
    {
        public string Name { get; }

        public ClassName(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString()
        {
            return Name.Replace("/", ".");
        }

        protected bool Equals(ClassName other)
        {
            return Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ClassName) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
