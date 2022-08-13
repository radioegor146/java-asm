namespace JavaAsm.IO.ConstantPoolEntries {
    internal enum EntryTag : byte {
        Class = 7,
        FieldReference = 9,
        MethodReference = 10,
        InterfaceMethodReference = 11,
        String = 8,
        Integer = 3,
        Float = 4,
        Long = 5,
        Double = 6,
        NameAndType = 12,
        Utf8 = 1,
        MethodHandle = 15,
        MethodType = 16,
        InvokeDynamic = 18
    }
}