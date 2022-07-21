using System;
using System.Collections.Generic;
using System.Linq;
using JavaAsm.CustomAttributes;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.Instructions;
using JavaAsm.IO;

namespace JavaAsm
{
    /// <summary>
    /// Method node
    /// </summary>
    public class MethodNode
    {
        /// <summary>
        /// Owner class of that method
        /// </summary>
        public ClassNode Owner { get; set; }


        /// <summary>
        /// Access flags
        /// </summary>
        public MethodAccessModifiers Access { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descriptor
        /// </summary>
        public MethodDescriptor Descriptor { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();


        /// <summary>
        /// Signature
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// Max size of stack
        /// </summary>
        public ushort MaxStack { get; set; }

        /// <summary>
        /// Max number of locals
        /// </summary>
        public ushort MaxLocals { get; set; }

        /// <summary>
        /// try-catch nodes
        /// </summary>
        public List<TryCatchNode> TryCatches { get; set; } = new List<TryCatchNode>();

        /// <summary>
        /// Instructions list
        /// </summary>
        public InstructionList Instructions { get; set; } = new InstructionList();

        /// <summary>
        /// Method's Code attribute attributes
        /// </summary>
        public List<AttributeNode> CodeAttributes { get; set; } = new List<AttributeNode>();

        /// <summary>
        /// Invisible annotations
        /// </summary>
        public List<AnnotationNode> InvisibleAnnotations { get; set; } = new List<AnnotationNode>();

        /// <summary>
        /// Visible annotations
        /// </summary>
        public List<AnnotationNode> VisibleAnnotations { get; set; } = new List<AnnotationNode>();

        /// <summary>
        /// Deprecated flag
        /// </summary>
        public bool IsDeprecated { get; set; }

        /// <summary>
        /// All throw statements
        /// </summary>
        public List<ClassName> Throws { get; set; } = new List<ClassName>();

        /// <summary>
        /// Default value of that annotation field if owner class is annotation
        /// </summary>
        public ElementValue AnnotationDefaultValue { get; set; }

        /// <summary>
        /// Returns and deletes attribute. Used for internal methods to parse contents
        /// </summary>
        /// <param name="name">Name of annotation</param>
        /// <returns>null, if attribute does not exist or AttributeNode if exists</returns>
        private AttributeNode GetAttribute(string name)
        {
            AttributeNode attribute = this.Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                this.Attributes.Remove(attribute);
            return attribute;
        }

        /// <summary>
        /// Parses method annotations to fill up information
        /// </summary>
        /// <param name="readerState">Class reader state</param>
        internal void Parse(ClassReaderState readerState)
        {
            this.Signature = (GetAttribute(PredefinedAttributeNames.Signature)?.ParsedAttribute as SignatureAttribute)?.Value;
            {
                AttributeNode attribute = GetAttribute(PredefinedAttributeNames.Code);
                if (attribute?.ParsedAttribute is CodeAttribute codeAttribute)
                    InstructionListConverter.ParseCodeAttribute(this, readerState, codeAttribute);
            }
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
            {
                AttributeNode attribute = GetAttribute(PredefinedAttributeNames.Exceptions);
                if (attribute != null)
                    this.Throws = (attribute.ParsedAttribute as ExceptionsAttribute)?.ExceptionTable;
            }
            this.AnnotationDefaultValue =
                (GetAttribute(PredefinedAttributeNames.AnnotationDefault)?.ParsedAttribute as AnnotationDefaultAttribute)?.Value;
            this.IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
        }

        /// <summary>
        /// Saves method information to annotations
        /// </summary>
        /// <param name="writerState">Class writer state</param>
        internal void Save(ClassWriterState writerState)
        {
            if (this.Signature != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Signature))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Signature} attribute is already presented on method");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Signature,
                    ParsedAttribute = new SignatureAttribute
                    {
                        Value = this.Signature
                    }
                });
            }

            if (!this.Access.HasFlag(MethodAccessModifiers.Abstract) && !this.Access.HasFlag(MethodAccessModifiers.Native) && this.Instructions != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Code))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Code} attribute is already presented on method");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Code,
                    ParsedAttribute = InstructionListConverter.SaveCodeAttribute(this, writerState)
                });
            }

            if (this.InvisibleAnnotations != null && this.InvisibleAnnotations.Count > 0)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeInvisibleAnnotations))
                    throw new Exception(
                        $"{PredefinedAttributeNames.RuntimeInvisibleAnnotations} attribute is already presented on method");
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
                        $"{PredefinedAttributeNames.RuntimeVisibleAnnotations} attribute is already presented on method");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.RuntimeVisibleAnnotations,
                    ParsedAttribute = new RuntimeVisibleAnnotationsAttribute
                    {
                        Annotations = this.VisibleAnnotations
                    }
                });
            }

            if (this.Throws.Count > 0)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Exceptions))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Exceptions} attribute is already presented on method");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Exceptions,
                    ParsedAttribute = new ExceptionsAttribute
                    {
                        ExceptionTable = this.Throws
                    }
                });
            }

            if (this.AnnotationDefaultValue != null)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.AnnotationDefault))
                    throw new Exception(
                        $"{PredefinedAttributeNames.AnnotationDefault} attribute is already presented on method");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.AnnotationDefault,
                    ParsedAttribute = new AnnotationDefaultAttribute
                    {
                        Value = this.AnnotationDefaultValue
                    }
                });
            }

            // ReSharper disable once InvertIf
            if (this.IsDeprecated)
            {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Deprecated))
                    throw new Exception(
                        $"{PredefinedAttributeNames.Deprecated} attribute is already presented on method");
                this.Attributes.Add(new AttributeNode
                {
                    Name = PredefinedAttributeNames.Deprecated,
                    ParsedAttribute = new DeprecatedAttribute()
                });
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{AccessModifiersExtensions.ToString(this.Access)} {this.Name}{this.Descriptor}";
        }
    }
}