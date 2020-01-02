using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            foreach (var entry in ZipFile.OpenRead("test4.jar").Entries)
            {
                if (!entry.FullName.EndsWith(".class"))
                    continue;
                using var entryStream = entry.Open();
                var result = ClassFile.ParseClass(entryStream);
                try
                {
                    // Console.WriteLine(result);
                } 
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to parse {entry.FullName}: {e}");
                }
            }
        }
    }
}
