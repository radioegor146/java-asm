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
            using var inputJarFile = new ZipArchive(new FileStream("DivineRPG-1.4.1.4-FP.jar", FileMode.Open), 
                ZipArchiveMode.Read, false);

            foreach (var inputEntry in inputJarFile.Entries)
            {
                if (inputEntry.Name == "")
                    continue;
                using var inputEntryStream = inputEntry.Open();
                if (!inputEntry.FullName.EndsWith(".class")) 
                    continue;
                var result = ClassFile.ParseClass(inputEntryStream);
                if (result.SuperName.Name.ToLower().Contains("boss"))
                    Console.WriteLine(result.Name);
            }
        }
    }
}
