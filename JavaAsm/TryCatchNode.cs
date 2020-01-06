using JavaAsm.Instructions.Types;

namespace JavaAsm
{
    public class TryCatchNode
    {
        public Label Start { get; set; }

        public Label End { get; set; }

        public Label Handler { get; set; }

        public ClassName ExceptionClassName { get; set; }
    }
}