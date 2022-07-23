using System;
using System.Collections.Generic;
using System.Linq;
using JavaAsm.CustomAttributes;
using JavaAsm.CustomAttributes.Annotation;
using JavaAsm.IO;

namespace JavaAsm {
    /// <summary>
    /// Field node
    /// </summary>
    public class FieldNode {
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
        private AttributeNode GetAttribute(string name) {
            AttributeNode attribute = this.Attributes.FirstOrDefault(a => a.Name == name);
            if (attribute != null)
                this.Attributes.Remove(attribute);
            return attribute;
        }

        /// <summary>
        /// Parses field annotations to fill up information
        /// </summary>
        /// <param name="readerState">Class reader state</param>
        internal void Parse(ClassReaderState readerState) {
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
            this.ConstantValue =
                (GetAttribute(PredefinedAttributeNames.ConstantValue)?.ParsedAttribute as
                    ConstantValueAttribute)?.Value;
            this.IsDeprecated = GetAttribute(PredefinedAttributeNames.Deprecated)?.ParsedAttribute != null;
        }

        /// <summary>
        /// Saves method information to annotations
        /// </summary>
        /// <param name="writerState">Class writer state</param>
        internal void Save(ClassWriterState writerState) {
            if (this.Signature != null) {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Signature))
                    throw new Exception($"{PredefinedAttributeNames.Signature} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.Signature,
                    ParsedAttribute = new SignatureAttribute {
                        Value = this.Signature
                    }
                });
            }

            if (this.InvisibleAnnotations != null && this.InvisibleAnnotations.Count > 0) {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeInvisibleAnnotations))
                    throw new Exception($"{PredefinedAttributeNames.RuntimeInvisibleAnnotations} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.RuntimeInvisibleAnnotations,
                    ParsedAttribute = new RuntimeInvisibleAnnotationsAttribute {
                        Annotations = this.InvisibleAnnotations
                    }
                });
            }

            if (this.VisibleAnnotations != null && this.VisibleAnnotations.Count > 0) {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.RuntimeVisibleAnnotations))
                    throw new Exception($"{PredefinedAttributeNames.RuntimeVisibleAnnotations} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.RuntimeVisibleAnnotations,
                    ParsedAttribute = new RuntimeVisibleAnnotationsAttribute {
                        Annotations = this.VisibleAnnotations
                    }
                });
            }

            if (this.ConstantValue != null) {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.ConstantValue))
                    throw new Exception($"{PredefinedAttributeNames.ConstantValue} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.ConstantValue,
                    ParsedAttribute = new ConstantValueAttribute {
                        Value = this.ConstantValue
                    }
                });
            }

            // ReSharper disable once InvertIf
            if (this.IsDeprecated) {
                if (this.Attributes.Any(x => x.Name == PredefinedAttributeNames.Deprecated))
                    throw new Exception($"{PredefinedAttributeNames.Deprecated} attribute is already presented on field");
                this.Attributes.Add(new AttributeNode {
                    Name = PredefinedAttributeNames.Deprecated,
                    ParsedAttribute = new DeprecatedAttribute()
                });
            }
        }

        /// <inheritdoc />
        public override string ToString() {
            return $"{AccessModifiersExtensions.ToString(this.Access)} {this.Descriptor} {this.Name}";
        }
    }
}