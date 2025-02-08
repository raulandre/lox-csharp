namespace Interpreter;

public class Env
{
    private Env enclosing;
    private Dictionary<string, object> values = new();

    public Env()
    {
        enclosing = null;
    }

    public Env(Env enclosing)
    {
        this.enclosing = enclosing;
    }

    public void Define(string name, object value)
    {
        values.Add(name, value);
    }

    public void Assign(Token name, object value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (enclosing != null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}.'");
    }

    public object Get(Token name)
    {
        if (values.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        if (enclosing != null)
            return enclosing.Get(name);

        throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}.'");
    }
}
