using System;
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

        public ClassAccessModifiers Access { get; set; }

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

        internal void Parse(ClassReaderState readerState)
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
            {
                var attribute = GetAttribute(PredefinedAttributeNames.InnerClasses);
                if (attribute != null)
                    InnerClasses = (attribute.ParsedAttribute as InnerClassesAttribute)?.Classes;
            }

            foreach (var method in Methods)
                method.Parse(readerState);

            foreach (var field in Fields)
                field.Parse(readerState);
        }

        internal void Save(ClassWriterState writerState)
        {
            if (SourceFile != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.SourceFile))
                    throw new Exception(
                        $"{PredefinedAttributeNames.SourceFile} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.SourceFile,
                    ParsedAttribute = new SourceFileAttribute
                    {
                        Value = SourceFile
                    }
                });
            }

            if (SourceDebugExtension != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.SourceDebugExtension))
                    throw new Exception(
                        $"{PredefinedAttributeNames.SourceDebugExtension} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.SourceDebugExtension,
                    ParsedAttribute = new SourceDebugExtensionAttribute
                    {
                        Value = SourceDebugExtension
                    }
                });
            }

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

            if (EnclosingMethod != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.EnclosingMethod))
                    throw new Exception(
                        $"{PredefinedAttributeNames.EnclosingMethod} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.EnclosingMethod,
                    ParsedAttribute = EnclosingMethod
                });
            }

            if (InnerClasses != null)
            {
                if (Attributes.Any(x => x.Name == PredefinedAttributeNames.InnerClasses))
                    throw new Exception(
                        $"{PredefinedAttributeNames.InnerClasses} attribute is already presented on field");
                Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.InnerClasses,
                    ParsedAttribute = new InnerClassesAttribute
                    {
                        Classes = InnerClasses
                    }
                });
            }

            foreach (var method in Methods)
                method.Save(writerState);

            foreach (var field in Fields)
                field.Save(writerState);
        }

        public override string ToString()
        {
            return $"{AccessModifiersExtensions.ToString(Access)} {Name}";
        }
    }
}
