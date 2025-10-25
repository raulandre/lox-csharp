using Lox.Visitors;

namespace Lox;

public interface ICallable
{
    public object? Call(Interpreter interpreter, IEnumerable<object?> args);
    public int Arity();
}