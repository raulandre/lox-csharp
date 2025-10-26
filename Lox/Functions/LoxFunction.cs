using Lox.Generated;
using Lox.OOP;
using Lox.Visitors;

namespace Lox.Functions;

public class LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer) : ICallable
{
    public Stmt.Function Declaration { get; private set; } = declaration;
    public Environment Closure { get; private set; } = closure;
    public bool IsInitializer { get; private set; } = isInitializer;

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
            if (IsInitializer) return Closure.GetAt(0, "this");
            return @return.Value;
        }

        if (IsInitializer) return Closure.GetAt(0, "this");
        return null;
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Environment(Closure);
        environment.Define("this", instance);
        return new LoxFunction(Declaration, environment, IsInitializer);
    }

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme}/{Arity()}>";
    }
}