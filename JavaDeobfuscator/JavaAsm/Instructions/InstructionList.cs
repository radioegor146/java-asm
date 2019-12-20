using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JavaDeobfuscator.JavaAsm.Instructions
{
    internal class InstructionList : IEnumerable<Instruction>
    {
        private class InstructionListEnumerator : IEnumerator<Instruction>
        {
            public InstructionListEnumerator(Instruction start)
            {
                Start = start;
            }

            public bool MoveNext()
            {
                if (Current == null)
                    return false;
                Current = Current.Next;
                return true;
            }

            public void Reset()
            {
                Current = Start;
            }

            public Instruction Current { get; private set; }

            private Instruction Start { get; }

            object IEnumerator.Current => Current;

            public void Dispose() { }
        }

        public Instruction First { get; private set; }

        public Instruction Last { get; private set; }

        public void Add(Instruction insnNode)
        {
            insnNode = (Instruction) insnNode.Clone();
            insnNode.OwnerList = this;
            insnNode.Next = null;
            if (First == null)
                First = insnNode;
            insnNode.Previous = Last;
            Last = insnNode;
            if (insnNode.Previous != null)
                insnNode.Previous.Next = insnNode;
        }

        public void InsertBefore(Instruction insnNode, Instruction toInsert)
        {
            if (insnNode.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list");
            toInsert = (Instruction) toInsert.Clone();
            toInsert.OwnerList = this;
            toInsert.Next = insnNode;
            toInsert.Previous = insnNode.Previous;

            if (toInsert.Previous != null)
                toInsert.Previous.Next = toInsert;
            toInsert.Next.Previous = toInsert;

            if (ReferenceEquals(insnNode, First))
                First = toInsert;
        }

        public void InsertAfter(Instruction insnNode, Instruction toInsert)
        {
            if (insnNode.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list");
            toInsert = (Instruction) toInsert.Clone();
            toInsert.OwnerList = this;
            toInsert.Previous = insnNode;
            toInsert.Next = insnNode.Next;

            if (toInsert.Next != null)
                toInsert.Next.Previous = toInsert;
            toInsert.Previous.Next = toInsert;

            if (ReferenceEquals(insnNode, Last))
                Last = toInsert;
        }

        public void Remove(Instruction insnNode)
        {
            if (insnNode.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list");
            insnNode.OwnerList = null;
            if (insnNode.Next != null)
                insnNode.Next.Previous = insnNode.Previous;
            if (insnNode.Previous != null)
                insnNode.Previous.Next = insnNode.Next;
            if (ReferenceEquals(insnNode, First))
                First = insnNode.Next;
            if (ReferenceEquals(insnNode, Last))
                Last = insnNode.Previous;
        }

        public IEnumerator<Instruction> GetEnumerator()
        {
            return new InstructionListEnumerator(First);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
