using System.Collections.Generic;
using System.Linq;
using JavaDeobfuscator.JavaAsm.CustomAttributes;
using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
using JavaDeobfuscator.JavaAsm.Instructions;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm
{
    internal class MethodNode
    {
        public ClassNode Owner { get; set; }


        public AccessModifiers Access { get; set; }

        public string Name { get; set; }

        public MethodDescriptor Descriptor { get; set; }

        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();


        public string Signature { get; set; }

        public ushort MaxStack { get; set; }

        public ushort MaxLocals { get; set; }

        public List<TryCatchNode> TryCatches { get; set; } = new List<TryCatchNode>();

        public InstructionList Instructions { get; set; } = new InstructionList();

        public List<AttributeNode> CodeAttributes { get; set; } = new List<AttributeNode>();

        public List<AnnotationNode> InvisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public List<AnnotationNode> VisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public bool IsDeprecated { get; set; }

        public List<ClassName> Throws { get; set; } = new List<ClassName>();

        public ElementValue AnnotationDefaultValue { get; set; }

        private AttributeNode GetAttribute(string name)
        {
            var attribute = Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                Attributes.Remove(attribute);
            return attribute;
        }

        internal void ParseAttributes(ClassReaderState readerState)
        {
            Signature = (GetAttribute(PredefinedAttributeNames.Signature)?.ParsedAttribute as SignatureAttribute)?.Value;
            {
                var attribute = GetAttribute(PredefinedAttributeNames.Code);
                if (attribute?.ParsedAttribute is CodeAttribute codeAttribute)
                    InstructionListConverter.ParseInstructionList(this, readerState, codeAttribute);
            }
            {
                var attribute = GetAttribute(PredefinedAttributeNames.RuntimeInvisibleAnnotations);
                if (attribute != null)
                    InvisibleAnnotations = (attribute.ParsedAttribute as RuntimeInvisibleAnnotationsAttribute)?.Annotations;
            }
            {
                var attribute = GetAttribute(PredefinedAttributeNames.RuntimeVisibleAnnotations);
                if (attribute != null)
                    VisibleAnnotations = (attribute.ParsedAttribute as RuntimeVisibleAnnotationsAttribute)?.Annotations;
            }
            {
                var attribute = GetAttribute(PredefinedAttributeNames.Exceptions);
                if (attribute != null)
                    Throws = (attribute.ParsedAttribute as ExceptionAttribute)?.ExceptionTable;
            }
            AnnotationDefaultValue =
                (GetAttribute(PredefinedAttributeNames.AnnotationDefault)?.ParsedAttribute as AnnotationDefaultAttribute)?.Value;
            IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
        }
    }
}