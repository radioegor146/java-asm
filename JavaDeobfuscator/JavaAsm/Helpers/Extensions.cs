using System;
using System.Linq;
using System.Collections.Generic;
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
    }
}
