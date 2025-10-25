namespace Lox.Functions;

public class Return(object? value) : Exception
{
    public object? Value { get; private set; } = value;
}