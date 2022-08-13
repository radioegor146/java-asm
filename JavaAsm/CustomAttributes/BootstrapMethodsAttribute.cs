using System;
using System.Collections.Generic;
using System.IO;
using BinaryEncoding;
using JavaAsm.Instructions.Types;
using JavaAsm.IO;
using JavaAsm.IO.ConstantPoolEntries;

namespace JavaAsm.CustomAttributes {
    public class BootstrapMethod {
        public Handle BootstrapMethodReference { get; }

        public List<object> Arguments { get; } = new List<object>();

        public BootstrapMethod(Handle bootstrapMethodReference) {
            this.BootstrapMethodReference = bootstrapMethodReference;
        }

        public BootstrapMethod(Handle bootstrapMethodReference, List<object> arguments) {
            this.BootstrapMethodReference = bootstrapMethodReference;
            this.Arguments = arguments;
        }

        public bool Equals(BootstrapMethod other) {
            return this.BootstrapMethodReference.Equals(other.BootstrapMethodReference) && this.Arguments.Equals(other.Arguments);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((BootstrapMethod) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return (this.BootstrapMethodReference.GetHashCode() * 397) ^ this.Arguments.GetHashCode();
            }
        }
    }


    internal class BootstrapMethodsAttribute : CustomAttribute {
        public List<BootstrapMethod> BootstrapMethods { get; set; } = new List<BootstrapMethod>();

        internal override byte[] Save(ClassWriterState writerState, AttributeScope scope) {
            using (MemoryStream attributeDataStream = new MemoryStream()) {
                if (this.BootstrapMethods.Count > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(this.BootstrapMethods.Count), $"Number of bootstrap methods is too big: {this.BootstrapMethods.Count} > {ushort.MaxValue}");
                Binary.BigEndian.Write(attributeDataStream, (ushort) this.BootstrapMethods.Count);
                foreach (BootstrapMethod method in this.BootstrapMethods) {
                    Binary.BigEndian.Write(attributeDataStream,
                        writerState.ConstantPool.Find(method.BootstrapMethodReference.ToConstantPool()));
                    if (method.Arguments.Count > ushort.MaxValue)
                        throw new ArgumentOutOfRangeException(nameof(method.Arguments.Count),
                            $"Number of arguments is too big: {method.Arguments.Count} > {ushort.MaxValue}");
                    Binary.BigEndian.Write(attributeDataStream, (ushort) method.Arguments.Count);
                    foreach (object argument in method.Arguments) {
                        Entry val;
                        if (argument is int) {
                            val = new IntegerEntry((int) argument);
                        }
                        else if (argument is float) {
                            val = new FloatEntry((float) argument);
                        }
                        else if (argument is string) {
                            val = new StringEntry(new Utf8Entry((string) argument));
                        }
                        else if (argument is long) {
                            val = new LongEntry((long) argument);
                        }
                        else if (argument is double) {
                            val = new DoubleEntry((double) argument);
                        }
                        else if (argument is Handle) {
                            val = ((Handle) argument).ToConstantPool();
                        }
                        else if (argument is MethodDescriptor) {
                            val = new MethodTypeEntry(new Utf8Entry(((MethodDescriptor) argument).ToString()));
                        }
                        else {
                            throw new ArgumentOutOfRangeException(nameof(argument), $"Can't encode value of type {argument.GetType()}");
                        }

                        Binary.BigEndian.Write(attributeDataStream, writerState.ConstantPool.Find(val));
                    }
                }

                return attributeDataStream.ToArray();
            }
        }
    }

    internal class BootstrapMethodsAttributeFactory : ICustomAttributeFactory<BootstrapMethodsAttribute> {
        public BootstrapMethodsAttribute Parse(Stream attributeDataStream, uint attributeDataLength, ClassReaderState readerState, AttributeScope scope) {
            BootstrapMethodsAttribute attribute = new BootstrapMethodsAttribute();

            ushort bootstrapMethodsCount = Binary.BigEndian.ReadUInt16(attributeDataStream);
            attribute.BootstrapMethods.Capacity = bootstrapMethodsCount;
            for (int i = 0; i < bootstrapMethodsCount; i++) {
                BootstrapMethod bootstrapMethod = new BootstrapMethod(
                    Handle.FromConstantPool(
                        readerState.ConstantPool.GetEntry<MethodHandleEntry>(
                            Binary.BigEndian.ReadUInt16(attributeDataStream))));
                ushort numberOfArguments = Binary.BigEndian.ReadUInt16(attributeDataStream);
                bootstrapMethod.Arguments.Capacity = numberOfArguments;
                for (int j = 0; j < numberOfArguments; j++) {
                    Entry argumentValueEntry =
                        readerState.ConstantPool.GetEntry<Entry>(Binary.BigEndian.ReadUInt16(attributeDataStream));
                    object item;
                    if (argumentValueEntry is StringEntry se) {
                        item = se.Value.String;
                    }
                    else if (argumentValueEntry is ClassEntry ce) {
                        item = new ClassName(ce.Name.String);
                    }
                    else if (argumentValueEntry is IntegerEntry ie) {
                        item = ie.Value;
                    }
                    else if (argumentValueEntry is LongEntry le) {
                        item = le.Value;
                    }
                    else if (argumentValueEntry is FloatEntry fe) {
                        item = fe.Value;
                    }
                    else if (argumentValueEntry is DoubleEntry de) {
                        item = de.Value;
                    }
                    else if (argumentValueEntry is MethodHandleEntry mhe) {
                        item = Handle.FromConstantPool(mhe);
                    }
                    else if (argumentValueEntry is MethodTypeEntry mte) {
                        item = MethodDescriptor.Parse(mte.Descriptor.String);
                    }
                    else {
                        throw new ArgumentOutOfRangeException(nameof(argumentValueEntry), $"Entry of type {argumentValueEntry.Tag} is not supported for argument of bootstrap method");
                    }

                    bootstrapMethod.Arguments.Add(item);
                }

                attribute.BootstrapMethods.Add(bootstrapMethod);
            }

            return attribute;
        }
    }
}