using Lox.Visitors;

namespace Lox.Functions;

public class Read : ICallable
{
    public int Arity() => 0;

    public object? Call(Interpreter interpreter, IEnumerable<object?> args)
    {
#if DEBUG
        static string? readLine() => Console.ReadLine();
#else
        static string readLine() => ReadLine.Read("");
#endif
        return readLine();
    }

    public override string ToString()
    {
        return $"<native fn {nameof(Read)}/{Arity()}>";
    }
}