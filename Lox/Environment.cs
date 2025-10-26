using Lox.Tokens;

namespace Lox;

public class Environment
{
    public Environment? Enclosing { get; private set; }
    public readonly Dictionary<string, object?> _values = [];

    public Environment()
    {
        Enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        Enclosing = enclosing;
    }

    public void Define(string name, object? value)
        => _values.Add(name, value);

    public object? Get(Token name)
    {
        if (_values.TryGetValue(name.Lexeme, out var value))
            return value;

        if (Enclosing is not null)
            return Enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme))
        {
            _values[name.Lexeme] = value;
            return;
        }

        if (Enclosing is not null)
        {
            Enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)?._values[name];
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)!._values[name.Lexeme] = value;
    }

    public Environment? Ancestor(int distance)
    {
        var environment = this;

        for (int i = 0; i < distance; i++)
        {
            environment = environment?.Enclosing;
        }

        return environment;
    }
}