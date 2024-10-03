namespace RR.Core.Editor.CodeGeneration
{
    public enum ContentType
    {
        @struct = 0,
        @class = 1,
        @enum = 2
    }

    public enum AccessLevel
    {
        @public = 0,
        @protected = 1,
        @private = 2
    }
    
    public enum Modifier
    {
        @override = 0,
        @virtual = 1,
        @abstract = 2
    }
}