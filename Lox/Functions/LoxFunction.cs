using Lox.Generated;
using Lox.Visitors;

namespace Lox.Functions;

public class LoxFunction(Stmt.Function declaration, Environment closure) : ICallable
{
    public Stmt.Function Declaration { get; private set; } = declaration;
    public Environment Closure { get; private set; } = closure;

    public int Arity()
        => Declaration.Params.Count;

    public object? Call(Interpreter interpreter, IEnumerable<object?> args)
    {
        var env = new Environment(Closure);
        foreach (var arg in args.Index())
            env.Define(Declaration.Params[arg.Index].Lexeme, arg.Item);

        try
        {
            interpreter.ExecuteBlock(Declaration.Body, env);
        }
        catch (Return @return)
        {
            return @return.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme}/{Arity()}>";
    }
}