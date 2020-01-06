    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
    using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm.CustomAttributes
{
    internal class AnnotationDefaultAttribute : CustomAttribute
    {
        public ElementValue Value { get; set; }

        public override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            using var attributeDataStream = new MemoryStream();

            Value.Write(attributeDataStream, writerState);

            return attributeDataStream.ToArray();
        }
    }

    internal class AnnotationDefaultAttributeFactory : ICustomAttributeFactory<AnnotationDefaultAttribute>
    {
        public AnnotationDefaultAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope)
        {
            return new AnnotationDefaultAttribute
            {
                Value = ElementValue.Parse(attributeDataStream, readerState)
            };
        }
    }
}
