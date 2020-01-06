using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using JavaAsm.IO;

namespace JavaDeobfuscator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            using var inputJarFile = new ZipArchive(new FileStream("BytecodeViewer.jar", FileMode.Open), 
                ZipArchiveMode.Read, false);
            using var outputJarFile = new ZipArchive(new FileStream("BytecodeViewer-d.jar", FileMode.Create),
                ZipArchiveMode.Create, false);

            foreach (var inputEntry in inputJarFile.Entries)
            {
                if (inputEntry.Name == "")
                    continue;
                using var inputEntryStream = inputEntry.Open();
                using var outputEntryStream = outputJarFile.CreateEntry(inputEntry.FullName).Open();
                if (!inputEntry.FullName.EndsWith(".class"))
                {
                    inputEntryStream.CopyTo(outputEntryStream);
                    continue;
                }

                var result = ClassFile.ParseClass(inputEntryStream);
                var dataStream = new MemoryStream();
                ClassFile.WriteClass(dataStream, result);
                dataStream.Position = 0;
                ClassFile.ParseClass(dataStream);
                dataStream.Position = 0;
                dataStream.CopyTo(outputEntryStream);
            }
        }
    }
}
