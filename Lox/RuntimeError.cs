using Lox.Tokens;

namespace Lox;

public class RuntimeError : Exception
{
    public Token Token { get; private set; }

    public RuntimeError(Token token, string message)
        : base(message)
    {
        Token = token;
    }
}