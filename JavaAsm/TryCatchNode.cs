using JavaAsm.Instructions.Types;

namespace JavaAsm {
    /// <summary>
    /// try-catch node
    /// </summary>
    public class TryCatchNode {
        /// <summary>
        /// Start label of try-catch block
        /// </summary>
        public Label Start { get; set; }

        /// <summary>
        /// End label of try-catch block
        /// </summary>
        public Label End { get; set; }

        /// <summary>
        /// Exception handler label of try-catch block
        /// </summary>
        public Label Handler { get; set; }

        /// <summary>
        /// Exception's class name (or null if that try-catch catches all exceptions)
        /// </summary>
        public ClassName ExceptionClassName { get; set; }
    }
}