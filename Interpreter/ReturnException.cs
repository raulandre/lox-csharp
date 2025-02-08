namespace Interpreter;

public class ReturnException : RuntimeException
{
    public object Value { get; private set; }

    public ReturnException(object value)
        : base(null, null)
    {
        Value = value;
    }
}
