using System;
using System.Collections.Generic;
using System.IO;
using JavaAsm.Instructions.Types;

namespace JavaAsm.Helpers {
    internal static class CollectionUtils {
        // A small performance helper, because the one used before converted T[] to IEnumerable<T>
        public static bool Contains<T>(this T[] array, in T value) {
            return Contains(array, value, EqualityComparer<T>.Default);
        }

        public static bool Contains<T>(this T[] array, in T value, EqualityComparer<T> comparer) {
            for (int i = 0; i < array.Length; i++) {
                if (comparer.Equals(array[i], value)) {
                    return true;
                }
            }

            return false;
        }

        public static bool TryGetAt<T>(IList<T> list, int index, out T value) {
            if (index < 0 || index >= list.Count) {
                value = default;
                return false;
            }
            else {
                value = list[index];
                return true;
            }
        }

        public static bool TryGetAtNonNull<T>(IList<T> list, int index, out T value) where T : class {
            if (index < 0 || index >= list.Count) {
                value = default;
                return false;
            }
            else {
                value = list[index];
                return value != null;
            }
        }
    }

    internal static class Extensions {
        public static char[] Repeat(this char character, int count) {
            char[] array = new char[count];
            for (int i = 0; i < count; ++i)
                array[i] = character;
            return array;
        }

        public static bool In<T>(this T value, params T[] values) {
            return values.Contains(value);
        }

        public static void CheckInAndThrow<T>(this T value, string valueName, params T[] values) {
            if (!values.Contains(value))
                throw new ArgumentOutOfRangeException(nameof(valueName), $"{valueName} is not in [{string.Join(", ", values)}]");
        }

        public static T VerifyOpcode<T>(this T value, string valueName, params T[] values) {
            if (!values.Contains(value))
                throw new ArgumentOutOfRangeException(nameof(valueName), $"{valueName} is not in [{string.Join(", ", values)}]");
            return value;
        }

        public static bool TryAdd<T>(this ICollection<T> collection, T value) {
            if (collection.Contains(value))
                return false;
            collection.Add(value);
            return true;
        }

        public static bool TryAdd<K, V>(this IDictionary<K, V> collection, K key, V value) {
            if (collection.ContainsKey(key))
                return false;
            collection[key] = value;
            return true;
        }
    }

    internal static class StreamExtensions {
        public static byte ReadByteFully(this Stream stream) {
            return stream.ReadBytes(1)[0];
        }

        public static byte[] ReadBytes(this Stream stream, long count) {
            byte[] buffer = new byte[count];
            int position = 0;
            while (position < buffer.Length) {
                int result = stream.Read(buffer, position, buffer.Length - position);
                if (result <= 0)
                    throw new EndOfStreamException();
                position += result;
            }

            return buffer;
        }

        public static void Write(this Stream stream, byte[] bytes) {
            stream.Write(bytes, 0, bytes.Length);
        }

        public static int Read(this Stream stream, byte[] bytes) {
            return stream.Read(bytes, 0, bytes.Length);
        }
    }

    internal static class ReferenceKindTypeExtensions {
        public static bool IsMethodReference(this ReferenceKindType referenceKindType) {
            return referenceKindType == ReferenceKindType.InvokeReference ||
                   referenceKindType == ReferenceKindType.InvokeSpecial ||
                   referenceKindType == ReferenceKindType.InvokeStatic ||
                   referenceKindType == ReferenceKindType.InvokeVirtual ||
                   referenceKindType == ReferenceKindType.NewInvokeSpecial;
        }

        public static bool IsFieldReference(this ReferenceKindType referenceKindType) {
            return referenceKindType == ReferenceKindType.GetField ||
                   referenceKindType == ReferenceKindType.GetStatic ||
                   referenceKindType == ReferenceKindType.PutField ||
                   referenceKindType == ReferenceKindType.PutStatic;
        }
    }

    internal static class DictionaryExtensions {
        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value) {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
            return dictionary[key];
        }

        public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TValue> provider) {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, provider());
            return dictionary[key];
        }
    }
}