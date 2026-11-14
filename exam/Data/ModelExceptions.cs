namespace exam.Data;

public sealed class ModelValidationException : Exception
{
    public ModelValidationException(string message) : base(message) { }
}

public sealed class DuplicateModelException : Exception
{
    public DuplicateModelException(string message) : base(message) { }
}
