namespace JavaAsm.Instructions {
    public abstract class Instruction {
        public InstructionList OwnerList { get; internal set; }

        public Instruction Previous { get; internal set; }

        public Instruction Next { get; internal set; }

        public abstract Opcode Opcode { get; set; }

        public abstract Instruction Copy();
    }
}