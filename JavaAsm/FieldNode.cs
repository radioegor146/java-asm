using System;
using System.Collections.Generic;
using System.Linq;
using JavaAsm.CustomAttributes;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm
{
    /// <summary>
    /// Field node
    /// </summary>
    public class FieldNode
    {
        /// <summary>
        /// Owner class
        /// </summary>
        public ClassNode Owner { get; set; }


        /// <summary>
        /// Access flags
        /// </summary>
        public FieldAccessModifiers Access { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Descriptor
        /// </summary>
        public TypeDescriptor Descriptor { get; set; }

        /// <summary>
        /// Attributes
        /// </summary>
        public List<AttributeNode> Attributes { get; set; } = new List<AttributeNode>();


        /// <summary>
        /// Signature
        /// </summary>
        public string Signature { get; set; }

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
        /// Constant value
        /// </summary>
        public object ConstantValue { get; set; }
        
        /// <summary>
        /// Returns and deletes attribute. Used for internal methods to parse contents
        /// </summary>
        /// <param name="name">Name of annotation</param>
        /// <returns>null, if attribute does not exist or AttributeNode if exists</returns>
        private AttributeNode GetAttribute(string name)
        {
            var attribute = Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                Attributes.Remove(attribute);
            return attribute;
        }
        
        /// <summary>
        /// Parses field annotations to fill up information
        /// </summary>
        /// <param name="readerState">Class reader state</param>
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

        /// <summary>
        /// Saves method information to annotations
        /// </summary>
        /// <param name="writerState">Class writer state</param>
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

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{AccessModifiersExtensions.ToString(Access)} {Descriptor} {Name}";
        }
    }
}