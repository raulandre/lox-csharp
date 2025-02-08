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
        return (double)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}

public class PrintFn : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> args)
    {
        var str = args.First().ToString();
        Console.WriteLine(str);
        return null;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}

public class ReadFn : ICallable
{
    public int Arity() => 0;

    public object Call(Interpreter interpreter, List<object> args)
    {
        return Console.ReadLine();
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}

public class ExitFn : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> args)
    {
        var code = Convert.ToInt32((double)args.First());
        Environment.Exit(code);
        return null;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}

public class NumberFn : ICallable
{
    public int Arity() => 1;

    public object Call(Interpreter interpreter, List<object> args)
    {
        var str = args.First() as string;


        if (str == null) return str;

        if (!double.TryParse(str, out var parsed))
            return null;

        return parsed;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
