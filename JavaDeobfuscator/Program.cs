using System;
using System.IO;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            {
                using var inputFileStream = new FileStream("NarratorWindows.class", FileMode.Open);
                var result = ClassFile.ParseClass(inputFileStream);
                using var outputFileStream = new FileStream("NarratorWindows-1.class", FileMode.Create);
                ClassFile.WriteClass(outputFileStream, result);
            }
            {
                using var inputFileStream = new FileStream("NarratorWindows-1.class", FileMode.Open);
                var result = ClassFile.ParseClass(inputFileStream);
            }
        }
    }
}
