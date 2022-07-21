using System.IO;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm.CustomAttributes
{
    public class AnnotationDefaultAttribute : CustomAttribute
    {
        public ElementValue Value { get; set; }

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope)
        {
            MemoryStream attributeDataStream = new MemoryStream();

            this.Value.Write(attributeDataStream, writerState);

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
