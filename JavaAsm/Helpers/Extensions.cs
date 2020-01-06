using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JavaAsm.Instructions.Types;

namespace JavaAsm.Helpers
{
    public static class Extensions
    {
        public static void CheckInAndThrow<T>(this T value, string valueName, params T[] values)
        {
            if (!values.Contains(value))
                throw new ArgumentOutOfRangeException(nameof(valueName), $"{valueName} is not in [{string.Join(", ", values)}]");
        }

        public static bool TryAdd<T>(this ICollection<T> collection, T value)
        {
            if (collection.Contains(value))
                return false;
            collection.Add(value);
            return true;
        }
    }

    public static class StreamExtensions
    {
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

    public static class ReferenceKindTypeExtensions
    {
        public static bool IsMethodReference(this ReferenceKindType referenceKindType)
        {
            return referenceKindType == ReferenceKindType.InvokeReference ||
                   referenceKindType == ReferenceKindType.InvokeSpecial ||
                   referenceKindType == ReferenceKindType.InvokeStatic ||
                   referenceKindType == ReferenceKindType.InvokeVirtual ||
                   referenceKindType == ReferenceKindType.NewInvokeSpecial;
        }

        public static bool IsFieldReference(this ReferenceKindType referenceKindType)
        {
            return referenceKindType == ReferenceKindType.GetField ||
                   referenceKindType == ReferenceKindType.GetStatic ||
                   referenceKindType == ReferenceKindType.PutField ||
                   referenceKindType == ReferenceKindType.PutStatic;
        }
    }

    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            return dictionary[key];
        }
    }
}
