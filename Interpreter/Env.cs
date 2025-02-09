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

    public void AssignAt(int distance, Token name, object value)
    {
        Ancestor(distance).values.Add(name.Lexeme, value);
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

    public object GetAt(int distance, string name)
    {
        var ancestor = Ancestor(distance);
        if (!ancestor.values.ContainsKey(name))
            return null;

        return ancestor.values[name];
    }

    public Env Ancestor(int distance)
    {
        var env = this;

        for (int i = 0; i < distance; i++)
            env = env.enclosing;

        return env;
    }
}
