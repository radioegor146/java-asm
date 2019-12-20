using System.Collections.Generic;
using System.Linq;
using System.Text;
using JavaDeobfuscator.JavaAsm.CustomAttributes;
using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm
{
    internal class ClassNode
    {
        public ClassVersion MajorVersion { get; set; }

        public ushort MinorVersion { get; set; }

        public AccessModifiers Access { get; set; }

        public ClassName Name { get; set; }

        public ClassName SuperName { get; set; }

        public List<ClassName> Interfaces { get; set; } = new List<ClassName>();

        public List<FieldNode> Fields { get; set; } = new List<FieldNode>();

        public List<MethodNode> Methods { get; set; } = new List<MethodNode>();

        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();


        public string SourceFile { get; set; }

        public string SourceDebugExtension { get; set; }

        public string Signature { get; set; }

        public List<AnnotationNode> InvisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public List<AnnotationNode> VisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public bool IsDeprecated { get; set; }

        public EnclosingMethodAttribute EnclosingMethod { get; set; }

        public List<InnerClass> InnerClasses { get; set; } = new List<InnerClass>();

        private AttributeNode GetAttribute(string name)
        {
            var attribute = Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                Attributes.Remove(attribute);
            return attribute;
        }

        internal void ParseAttributes(ClassReaderState readerState)
        {
            SourceFile = (GetAttribute(PredefinedAttributeNames.SourceFile)?.ParsedAttribute as SourceFileAttribute)?.Value;
            SourceDebugExtension = (GetAttribute(PredefinedAttributeNames.SourceDebugExtension)?.ParsedAttribute as SourceFileAttribute)?.Value;
            Signature = (GetAttribute(PredefinedAttributeNames.Signature)?.ParsedAttribute as SignatureAttribute)?.Value;
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
            IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
            EnclosingMethod = GetAttribute(PredefinedAttributeNames.EnclosingMethod)?.ParsedAttribute as EnclosingMethodAttribute;
            IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
            {
                var attribute = GetAttribute(PredefinedAttributeNames.InnerClasses);
                if (attribute != null)
                    InnerClasses = (attribute.ParsedAttribute as InnerClassesAttribute)?.Classes;
            }
        }
    }
}
