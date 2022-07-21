using System;
using System.Collections.Generic;
using System.Linq;
using JavaAsm.CustomAttributes;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm
{
    /// <summary>
    /// Class node
    /// </summary>
    public class ClassNode
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
            AttributeNode attribute = this.Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                this.Attributes.Remove(attribute);
            return attribute;
        }

        internal void Parse(ClassReaderState readerState)
        {
            this.SourceFile = (GetAttribute(PredefinedAttributeNames.SourceFile)?.ParsedAttribute as SourceFileAttribute)?.Value;
            this.SourceDebugExtension = (GetAttribute(PredefinedAttributeNames.SourceDebugExtension)?.ParsedAttribute as SourceFileAttribute)?.Value;
            this.Signature = (GetAttribute(PredefinedAttributeNames.Signature)?.ParsedAttribute as SignatureAttribute)?.Value;
            {
                AttributeNode attribute = GetAttribute(PredefinedAttributeNames.RuntimeInvisibleAnnotations);
                if (attribute != null)
                    this.InvisibleAnnotations = (attribute.ParsedAttribute as RuntimeInvisibleAnnotationsAttribute)?.Annotations;
            }
            {
                AttributeNode attribute = GetAttribute(PredefinedAttributeNames.RuntimeVisibleAnnotations);
                if (attribute != null)
                    this.VisibleAnnotations = (attribute.ParsedAttribute as RuntimeVisibleAnnotationsAttribute)?.Annotations;
            }
            this.IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
            this.EnclosingMethod = GetAttribute(PredefinedAttributeNames.EnclosingMethod)?.ParsedAttribute as EnclosingMethodAttribute;
            {
                AttributeNode attribute = GetAttribute(PredefinedAttributeNames.InnerClasses);
                if (attribute != null)
                    this.InnerClasses = (attribute.ParsedAttribute as InnerClassesAttribute)?.Classes;
            }

            foreach (MethodNode method in this.Methods)
                method.Parse(readerState);

            foreach (FieldNode field in this.Fields)
                field.Parse(readerState);
        }

        internal void Save(ClassWriterState writerState)
        {
            if (this.SourceFile != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.SourceFile))
                    throw new Exception(
                        $"{PredefinedAttributeNames.SourceFile} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.SourceFile,
                    ParsedAttribute = new SourceFileAttribute
                    {
                        Value = this.SourceFile
                    }
                });
            }

            if (this.SourceDebugExtension != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.SourceDebugExtension))
                    throw new Exception(
                        $"{PredefinedAttributeNames.SourceDebugExtension} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.SourceDebugExtension,
                    ParsedAttribute = new SourceDebugExtensionAttribute
                    {
                        Value = this.SourceDebugExtension
                    }
                });
            }

            if (this.Signature != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Signature))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Signature} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Signature,
                    ParsedAttribute = new SignatureAttribute
                    {
                        Value = this.Signature
                    }
                });
            }

            if (this.InvisibleAnnotations != null && this.InvisibleAnnotations.Count > 0)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeInvisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeInvisibleAnnotations} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeInvisibleAnnotations,
                    ParsedAttribute = new RuntimeInvisibleAnnotationsAttribute
                    {
                        Annotations = this.InvisibleAnnotations
                    }
                });
            }

            if (this.VisibleAnnotations != null && this.VisibleAnnotations.Count > 0)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeVisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeVisibleAnnotations} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeVisibleAnnotations,
                    ParsedAttribute = new RuntimeVisibleAnnotationsAttribute
                    {
                        Annotations = this.VisibleAnnotations
                    }
                });
            }

            if (this.IsDeprecated)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Deprecated))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Deprecated} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Deprecated,
                    ParsedAttribute = new DeprecatedAttribute()
                });
            }

            if (this.EnclosingMethod != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.EnclosingMethod))
                    throw new Exception(
                        $"{PredefinedAttributeNames.EnclosingMethod} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.EnclosingMethod,
                    ParsedAttribute = this.EnclosingMethod
                });
            }

            if (this.InnerClasses != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.InnerClasses))
                    throw new Exception(
                        $"{PredefinedAttributeNames.InnerClasses} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.InnerClasses,
                    ParsedAttribute = new InnerClassesAttribute
                    {
                        Classes = this.InnerClasses
                    }
                });
            }

            foreach (MethodNode method in this.Methods)
                method.Save(writerState);

            foreach (FieldNode field in this.Fields)
                field.Save(writerState);
        }

        public override string ToString()
        {
            return $"{AccessModifiersExtensions.ToString(this.Access)} {this.Name}";
        }
    }
}