using Lox.Functions;
using Lox.Visitors;

namespace Lox.OOP;

public class LoxClass(string name, Dictionary<string, LoxFunction> methods) : ICallable
{
    public string Name { get; private set; } = name;
    public Dictionary<string, LoxFunction> Methods { get; private set; } = methods;

    public LoxFunction? FindMethod(string name)
    {
        if (Methods.TryGetValue(name, out var method))
            return method;
        return null;
    }

    public int Arity()
    {
        var initializer = FindMethod("init");
        if (initializer is null) return 0;
        return initializer.Arity();
    }

    public object? Call(Interpreter interpreter, IEnumerable<object?> args)
    {
        var instance = new LoxInstance(this);
        var initializer = FindMethod("init");
        if (initializer is not null)
        {
            initializer.Bind(instance).Call(interpreter, args);
        }
        return instance;
    }

    public override string ToString()
    {
        return Name;
    }
}