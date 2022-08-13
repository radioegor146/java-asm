namespace JavaAsm.CustomAttributes.TypeAnnotation {
    public enum TargetType {
        // Type in ...

        GenericClassOrInterfaceDeclaration = 0x00,
        GenericMethodOrConstructorDeclaration = 0x01,
        ExtendsOrImplements = 0x10,
        TypeInBoundInGenericClassOrInterface = 0x11,
        TypeInBoundInGenericMethodOrConstructor = 0x12,
        FieldDeclaration = 0x13,
        ReturnTypeOrNewObject = 0x14,
        ReceiverTypeOfMethodOrConstructor = 0x15,
        TypeInFormalParameterOfMethodOrConstructorOrLambda = 0x16,
        ThrowsClause = 0x17,

        LocalVariableDeclaration = 0x40,
        ResourceVariableDeclaration = 0x41,
        ExceptionParameterDeclaration = 0x42,
        InstanceOfExpression = 0x43,
        NewExpression = 0x44,
        MethodReferenceExpressionNew = 0x45,
        MethodReferenceExpressionIdentifier = 0x46,
        CastExpression = 0x47,
        ArgumentForGenericConstructorInvocation = 0x48,
        ArgumentForGenericMethodInvocation = 0x49,
        ArgumentForGenericMethodReferenceExpressionNew = 0x4A,
        ArgumentForGenericMethodReferenceExpressionIdentifier = 0x4B
    }
}