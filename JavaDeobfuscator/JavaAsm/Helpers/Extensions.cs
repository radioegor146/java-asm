using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Helpers
{
    internal static class Extensions
    {
        public static void CheckInAndThrow<T>(this T value, string valueName, params T[] values)
        {
            if (!values.Contains(value))
                throw new ArgumentOutOfRangeException($"{valueName} is not in [{string.Join(", ", values)}]");
        }

        public static bool TryAdd<T>(this ICollection<T> collection, T value)
        {
            if (collection.Contains(value))
                return false;
            collection.Add(value);
            return true;
        }

        public static byte ReadByteFully(this Stream stream)
        {
            return stream.ReadBytes(1)[0];
        }

        public static byte[] ReadBytes(this Stream stream, long count)
        {
            var buffer = new byte[count];
            var position = 0;
            while (position < buffer.Length)
            {
                var result = stream.Read(buffer, position, buffer.Length - position);
                if (result <= 0)
                    throw new EndOfStreamException();
                position += result;
            }
            return buffer;
        }
    }
}
