using System;

namespace JavaAsm
{
    public class ClassName
    {
        public string Name { get; }

        public ClassName(string name)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public override string ToString()
        {
            return this.Name.Replace("/", ".");
        }

        private bool Equals(ClassName other)
        {
            return this.Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ClassName) obj);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}