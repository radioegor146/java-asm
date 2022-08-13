using System;
using System.Collections;
using System.Collections.Generic;

namespace JavaAsm.Instructions {
    public class InstructionList : IEnumerable<Instruction> {
        private class InstructionListEnumerator : IEnumerator<Instruction> {
            public InstructionListEnumerator(Instruction start) {
                this.Start = start;
            }

            public bool MoveNext() {
                if (this.Start != null && this.Current == null) {
                    this.Current = this.Start;
                    return true;
                }

                if (this.Current?.Next == null)
                    return false;
                this.Current = this.Current.Next;
                return true;
            }

            public void Reset() {
                this.Current = null;
            }

            public Instruction Current { get; private set; }

            private Instruction Start { get; }

            object IEnumerator.Current => this.Current;

            public void Dispose() { }
        }

        public Instruction First { get; private set; }

        public Instruction Last { get; private set; }

        public int Count { get; private set; }

        public void Add(Instruction instruction) {
            instruction.OwnerList = this;
            instruction.Next = null;
            if (this.First == null)
                this.First = instruction;
            instruction.Previous = this.Last;
            this.Last = instruction;
            if (instruction.Previous != null)
                instruction.Previous.Next = instruction;
            this.Count++;
        }

        public void InsertBefore(Instruction instruction, Instruction toInsert) {
            if (instruction.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list", nameof(instruction.OwnerList));
            toInsert.OwnerList = this;
            toInsert.Next = instruction;
            toInsert.Previous = instruction.Previous;

            if (toInsert.Previous != null)
                toInsert.Previous.Next = toInsert;
            toInsert.Next.Previous = toInsert;

            if (ReferenceEquals(instruction, this.First))
                this.First = toInsert;
            this.Count++;
        }

        public void InsertAfter(Instruction instruction, Instruction toInsert) {
            if (instruction.OwnerList != this)
                throw new ArgumentException("Position instruction does not belong to that list", nameof(instruction.OwnerList));
            toInsert.OwnerList = this;
            toInsert.Previous = instruction;
            toInsert.Next = instruction.Next;

            if (toInsert.Next != null)
                toInsert.Next.Previous = toInsert;
            toInsert.Previous.Next = toInsert;

            if (ReferenceEquals(instruction, this.Last))
                this.Last = toInsert;
            this.Count++;
        }

        public void Remove(Instruction instruction) {
            if (instruction.OwnerList != this)
                throw new ArgumentException("Instruction does not belong to that list", nameof(instruction.OwnerList));
            instruction.OwnerList = null;
            if (instruction.Next != null)
                instruction.Next.Previous = instruction.Previous;
            if (instruction.Previous != null)
                instruction.Previous.Next = instruction.Next;
            if (ReferenceEquals(instruction, this.First))
                this.First = instruction.Next;
            if (ReferenceEquals(instruction, this.Last))
                this.Last = instruction.Previous;
            this.Count--;
        }

        // public void Clear() {
        //     Instruction current = this.First;
        //     while (current != null) {
        //         Instruction temp = current;
        //         current = current.Next;
        //         temp.Next = null;
        //         temp.Previous = null;
        //         temp.OwnerList = null;
        //     }
        //     this.First = null;
        //     this.Count = 0;
        // }

        public IEnumerator<Instruction> GetEnumerator() {
            return new InstructionListEnumerator(this.First);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}