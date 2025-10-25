using Lox.Visitors;

namespace Lox.Functions;

public class Clock : ICallable
{
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, IEnumerable<object?> args)
        => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    public override string ToString()
    {
        return $"<native fn {nameof(Clock)}/{Arity()}>";
    }
}