using Lox.Visitors;

namespace Lox.Functions;

public class Number : ICallable
{
    public int Arity() => 1;

    public object? Call(Interpreter interpreter, IEnumerable<object?> args)
    {
        if (double.TryParse(args.First()?.ToString(), out var number))
            return number;

        return double.NaN;
    }
    public override string ToString()
    {
        return $"<native fn {nameof(Number)}/{Arity()}>";
    }
}