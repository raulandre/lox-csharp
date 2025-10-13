namespace Lox.Tokens;

public class Token(TokenType type, string lexeme, object? literal, int line)
{
    public TokenType Type { get; private set; } = type;
    public string Lexeme { get; private set; } = lexeme;
    public object? Literal { get; set; } = literal;
    public int Line { get; set; } = line;

    public override string ToString()
    {
        var type = Enum.GetName(Type);
        return $"{type} {Lexeme} {Literal}";
    }
}