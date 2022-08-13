﻿using System;

namespace JavaAsm.Instructions.Types {
    public class LineNumber : Instruction {
        public override Opcode Opcode {
            get => Opcode.None;
            set => throw new InvalidOperationException(GetType().Name + " does not have an instruction");
        }

        public override Instruction Copy() {
            return new LineNumber() {
                Line = this.Line
            };
        }

        public ushort Line { get; set; }

        public override string ToString() {
            return $"LINE {this.Line}";
        }
    }
}