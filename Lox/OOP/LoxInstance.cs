using Lox.Tokens;

namespace Lox.OOP;

public class LoxInstance(LoxClass @class)
{
    public LoxClass Class { get; private set; } = @class;
    public Dictionary<string, object?> Fields { get; private set; } = [];

    public object? Get(Token name)
    {
        if (Fields.TryGetValue(name.Lexeme, out object? value))
            return value;

        var method = Class.FindMethod(name.Lexeme);
        if (method is not null) return method.Bind(this);

        throw new RuntimeError(name, $"Undefined property {name.Lexeme}.");
    }

    public void Set(Token name, object? value)
    {
        Fields.Add(name.Lexeme, value);
    }

    public override string ToString()
        => $"{Class.Name} instance";
}