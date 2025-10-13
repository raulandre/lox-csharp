using System.Globalization;
using Lox.Tokens;

using static Lox.Tokens.TokenType;

namespace Lox;

public class Scanner(string source)
{
    public string Source { get; private set; } = source;
    public List<Token> Tokens { get; private set; } = [];

    public int Start { get; private set; } = 0;
    public int Current { get; private set; } = 0;
    public int Line { get; private set; } = 1;

    private static readonly Dictionary<string, TokenType> _keywords = new()
    {
        {"and", AND},
        {"class", CLASS},
        {"else", ELSE},
        {"false", FALSE},
        {"for", FOR},
        {"fun", FUN},
        {"if", IF},
        {"nil", NIL},
        {"or", OR},
        {"print", PRINT},
        {"return", RETURN},
        {"super", SUPER},
        {"this", THIS},
        {"true", TRUE},
        {"var", VAR},
        {"while", WHILE},
    };
    public static IReadOnlyDictionary<string, TokenType> Keywords => _keywords;

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            Start = Current;
            ScanToken();
        }

        Tokens.Add(new Token(EOF, string.Empty, null, Line));
        return Tokens;
    }

    private void ScanToken()
    {
        var c = Advance();

        switch (c)
        {
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '*': AddToken(STAR); break;
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            case '/':
                if (Match('/'))
                    while (Peek() != '\n' && !IsAtEnd())
                        Advance();
                else
                    AddToken(SLASH);
                break;
            case ' ':
            case '\r':
            case '\t':
                break;

            case '\n':
                Line++;
                break;

            case '"':
                Str();
                break;

            default:
                if (char.IsDigit(c))
                    Number();
                else if (char.IsLetter(c))
                    Identifier();
                else
                    Program.Error(Line, $"Unexpected character '{c}'.");
                break;
        }
    }

    private void Str()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') Line++;
            Advance();
        }

        if (IsAtEnd())
        {
            Program.Error(Line, "Unterminated string.");
            return;
        }

        // Consume the terminating '"'
        Advance();

        var str = Source[(Start + 1)..(Current - 1)];
        AddToken(STRING, str);
    }

    private void Number()
    {
        while (char.IsDigit(Peek())) Advance();

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            // Consume the '.'
            Advance();

            while (char.IsDigit(Peek())) Advance();
        }

        var parsed = double.Parse(Source[Start..Current], CultureInfo.InvariantCulture);
        AddToken(NUMBER, parsed);
    }

    private void Identifier()
    {
        while (char.IsLetter(Peek())) Advance();
        var text = Source[Start..Current];
        if (!Keywords.TryGetValue(text, out var type))
            type = IDENTIFIER;
        AddToken(type);
    }

    private bool IsAtEnd()
    {
        return Current >= Source.Length;
    }

    private char Advance()
    {
        return Source.ElementAt(Current++);
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (Source.ElementAt(Current) != expected) return false;

        Current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0';
        return Source.ElementAt(Current);
    }

    private char PeekNext()
    {
        if (Current + 1 >= Source.Length) return '\0';
        return Source.ElementAt(Current + 1);
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal)
    {
        var text = Source[Start..Current];
        Tokens.Add(new Token(type, text, literal, Line));
    }
}