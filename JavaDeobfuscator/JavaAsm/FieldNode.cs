using System;
using System.Collections.Generic;
using System.Linq;
using JavaDeobfuscator.JavaAsm.CustomAttributes;
using JavaDeobfuscator.JavaAsm.CustomAttributes.Annotation;
using JavaDeobfuscator.JavaAsm.IO;

namespace JavaDeobfuscator.JavaAsm
{
    internal class FieldNode
    {
        public ClassNode Owner { get; set; }


        public FieldAccessModifiers Access { get; set; }

        public string Name { get; set; }

        public TypeDescriptor Descriptor { get; set; }

        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();


        public string Signature { get; set; }

        public List<AnnotationNode> InvisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public List<AnnotationNode> VisibleAnnotations { get; set; } = new List<AnnotationNode>();

        public bool IsDeprecated { get; set; }

        public object ConstantValue { get; set; }

        private AttributeNode GetAttribute(string name)
        {
            var attribute = Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                Attributes.Remove(attribute);
            return attribute;
        }

        internal void Parse(ClassReaderState readerState)
        {
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
            ConstantValue =
                (GetAttribute(PredefinedAttributeNames.ConstantValue)?.ParsedAttribute as
                    ConstantValueAttribute)?.Value;
            IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
        }

        internal void Save(ClassWriterState writerState)
        {
            if (Signature != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.Signature))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Signature} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Signature,
                    ParsedAttribute = new SignatureAttribute
                    {
                        Value = Signature
                    }
                });
            }

            if (InvisibleAnnotations.Count > 0)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeInvisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeInvisibleAnnotations} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeInvisibleAnnotations,
                    ParsedAttribute = new RuntimeInvisibleAnnotationsAttribute
                    {
                        Annotations = InvisibleAnnotations
                    }
                });
            }

            if (VisibleAnnotations.Count > 0)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeVisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeVisibleAnnotations} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeVisibleAnnotations,
                    ParsedAttribute = new RuntimeVisibleAnnotationsAttribute
                    {
                        Annotations = VisibleAnnotations
                    }
                });
            }

            if (ConstantValue != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.ConstantValue))
                    throw new Exception(
                        $"{PredefinedAttributeNames.ConstantValue} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.ConstantValue,
                    ParsedAttribute = new ConstantValueAttribute
                    {
                        Value = ConstantValue
                    }
                });
            }

            // ReSharper disable once InvertIf
            if (IsDeprecated)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.Deprecated))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Deprecated} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Deprecated,
                    ParsedAttribute = new DeprecatedAttribute()
                });
            }
        }

        public override string ToString()
        {
            return $"{AccessModifiersExtensions.ToString(Access)} {Descriptor} {Name}";
        }
    }
}