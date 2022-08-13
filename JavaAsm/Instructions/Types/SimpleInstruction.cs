using JavaAsm.Helpers;

namespace JavaAsm.Instructions.Types {
    public class SimpleInstruction : Instruction {
        private static readonly Opcode[] ValidOpcodes = {
            Opcode.NOP, Opcode.ACONST_NULL, Opcode.ICONST_M1, Opcode.ICONST_0, Opcode.ICONST_1, Opcode.ICONST_2,
            Opcode.ICONST_3, Opcode.ICONST_4, Opcode.ICONST_5, Opcode.LCONST_0, Opcode.LCONST_1, Opcode.FCONST_0,
            Opcode.FCONST_1, Opcode.FCONST_2, Opcode.DCONST_0, Opcode.DCONST_1, Opcode.IALOAD, Opcode.LALOAD, Opcode.FALOAD,
            Opcode.DALOAD, Opcode.AALOAD, Opcode.BALOAD, Opcode.CALOAD, Opcode.SALOAD, Opcode.IASTORE, Opcode.LASTORE,
            Opcode.FASTORE, Opcode.DASTORE, Opcode.AASTORE, Opcode.BASTORE, Opcode.CASTORE, Opcode.SASTORE, Opcode.POP,
            Opcode.POP2, Opcode.DUP, Opcode.DUP_X1, Opcode.DUP_X2, Opcode.DUP2, Opcode.DUP2_X1, Opcode.DUP2_X2, Opcode.SWAP,
            Opcode.IADD, Opcode.LADD, Opcode.FADD, Opcode.DADD, Opcode.ISUB, Opcode.LSUB, Opcode.FSUB, Opcode.DSUB,
            Opcode.IMUL, Opcode.LMUL, Opcode.FMUL, Opcode.DMUL, Opcode.IDIV, Opcode.LDIV, Opcode.FDIV, Opcode.DDIV,
            Opcode.IREM, Opcode.LREM, Opcode.FREM, Opcode.DREM, Opcode.INEG, Opcode.LNEG, Opcode.FNEG, Opcode.DNEG,
            Opcode.ISHL, Opcode.LSHL, Opcode.ISHR, Opcode.LSHR, Opcode.IUSHR, Opcode.LUSHR, Opcode.IAND, Opcode.LAND,
            Opcode.IOR, Opcode.LOR, Opcode.IXOR, Opcode.LXOR, Opcode.I2L, Opcode.I2F, Opcode.I2D, Opcode.L2I, Opcode.L2F,
            Opcode.L2D, Opcode.F2I, Opcode.F2L, Opcode.F2D, Opcode.D2I, Opcode.D2L, Opcode.D2F, Opcode.I2B, Opcode.I2C,
            Opcode.I2S, Opcode.LCMP, Opcode.FCMPL, Opcode.FCMPG, Opcode.DCMPL, Opcode.DCMPG, Opcode.IRETURN, Opcode.LRETURN,
            Opcode.FRETURN, Opcode.DRETURN, Opcode.ARETURN, Opcode.RETURN, Opcode.ARRAYLENGTH, Opcode.ATHROW,
            Opcode.MONITORENTER, Opcode.MONITOREXIT
        };

        private Opcode opcode;

        public override Opcode Opcode {
            get => this.opcode;
            set => this.opcode = value.VerifyOpcode(nameof(value), ValidOpcodes);
        }

        public override Instruction Copy() {
            return new SimpleInstruction(this.opcode);
        }

        public SimpleInstruction(Opcode opcode) {
            this.Opcode = opcode;
        }

        public override string ToString() {
            return $"{this.Opcode}";
        }
    }
}