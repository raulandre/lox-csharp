namespace Interpreter;

public class LoxFunction : ICallable
{
    private Function Declaration;

    public LoxFunction(Function declaration)
    {
        Declaration = declaration;
    }

    public object Call(Interpreter interpreter, List<object> args)
    {
        var env = new Env(interpreter.globals);

        for (int i = 0; i < Declaration.Parameters.Count; i++)
        {
            env.Define(Declaration.Parameters[i].Lexeme, args[i]);
        }

        interpreter.ExecuteBlock(Declaration.Body, env);
        return null;
    }

    public int Arity() => Declaration.Parameters.Count;

    public override string ToString()
    {
        return $"<fn {Declaration.Name.Lexeme}>";
    }
}

public class ClockFn : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> args)
    {
        return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
