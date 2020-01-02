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
                Current = Start = start;
            }

            public bool MoveNext()
            {
                if (Current?.Next == null)
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

        public void Add(Instruction instruction)
        {
            instruction = (Instruction) instruction.Clone();
            instruction.OwnerList = this;
            instruction.Next = null;
            if (First == null)
                First = instruction;
            instruction.Previous = Last;
            Last = instruction;
            if (instruction.Previous != null)
                instruction.Previous.Next = instruction;
        }

        public void InsertBefore(Instruction instruction, Instruction toInsert)
        {
            if (instruction.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list");
            toInsert = (Instruction) toInsert.Clone();
            toInsert.OwnerList = this;
            toInsert.Next = instruction;
            toInsert.Previous = instruction.Previous;

            if (toInsert.Previous != null)
                toInsert.Previous.Next = toInsert;
            toInsert.Next.Previous = toInsert;

            if (ReferenceEquals(instruction, First))
                First = toInsert;
        }

        public void InsertAfter(Instruction instruction, Instruction toInsert)
        {
            if (instruction.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list");
            toInsert = (Instruction) toInsert.Clone();
            toInsert.OwnerList = this;
            toInsert.Previous = instruction;
            toInsert.Next = instruction.Next;

            if (toInsert.Next != null)
                toInsert.Next.Previous = toInsert;
            toInsert.Previous.Next = toInsert;

            if (ReferenceEquals(instruction, Last))
                Last = toInsert;
        }

        public void Remove(Instruction instruction)
        {
            if (instruction.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list");
            instruction.OwnerList = null;
            if (instruction.Next != null)
                instruction.Next.Previous = instruction.Previous;
            if (instruction.Previous != null)
                instruction.Previous.Next = instruction.Next;
            if (ReferenceEquals(instruction, First))
                First = instruction.Next;
            if (ReferenceEquals(instruction, Last))
                Last = instruction.Previous;
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
