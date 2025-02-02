namespace Interpreter;

public class Scanner
{
    public string Source { get; private set; }
    private readonly List<Token> Tokens = new();

    private int start = 0;
    private int current = 0;
    private int line = 1;

    private Dictionary<string, TokenType> keywords = new();

    public Scanner(string source)
    {
        Source = source;
        keywords.Add("and", TokenType.AND);
        keywords.Add("class", TokenType.CLASS);
        keywords.Add("else", TokenType.ELSE);
        keywords.Add("false", TokenType.FALSE);
        keywords.Add("for", TokenType.FOR);
        keywords.Add("fun", TokenType.FUN);
        keywords.Add("if", TokenType.IF);
        keywords.Add("nil", TokenType.NIL);
        keywords.Add("or", TokenType.OR);
        keywords.Add("print", TokenType.PRINT);
        keywords.Add("return", TokenType.RETURN);
        keywords.Add("super", TokenType.SUPER);
        keywords.Add("this", TokenType.THIS);
        keywords.Add("true", TokenType.TRUE);
        keywords.Add("var", TokenType.VAR);
        keywords.Add("while", TokenType.WHILE);
        keywords.Add("break", TokenType.BREAK);
    }

    public List<Token> ScanTokens() 
    {
        while(!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        Tokens.Add(new(TokenType.EOF, "", null, line));
        return Tokens;
    }

    private void ScanToken()
    {
        var c = Advance();

        switch(c)
        {
            case ' ': case '\r': case '\t': break;
            case '\n': line++; break;
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;
            case '!':
                AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '/':
                if(Match('/'))
                {
                    while(Peek() != '\n' && !IsAtEnd())
                        Advance();
                }
                else
                    AddToken(TokenType.SLASH);
                break;
            case '"':
                Str();
                break;
            default: 
                if(char.IsDigit(c))
                {
                    Num();
                }
                else if(char.IsLetterOrDigit(c) || c == '_')
                {
                    Id();
                }
                else
                {
                    Program.Error(line, "Unexpected character."); 
                }
                break;
        }
    }

    private bool IsAtEnd()
    {
        return current >= Source.Length;
    }

    private char Advance()
    {
        return Source.ElementAt(current++);
    }

    private void AddToken(TokenType type) 
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object literal)
    {
        var text = Source.Substring(start, current - start);
        Tokens.Add(new(type, text, literal, line));
    }

    private bool Match(char expected)
    {
        if(IsAtEnd()) return false;

        if(Source.ElementAt(current) != expected) return false;

        current++;
        return true;
    }

    private char Peek()
    {
        if(IsAtEnd()) return '\0';
        return Source.ElementAt(current);
    }

    private char PeekNext()
    {
        if(current + 1 >= Source.Length) return '\0';
        return Source.ElementAt(current + 1);
    }

    private void Str()
    {
        while(Peek() != '"' && !IsAtEnd())
        {
            if(Peek() == '\n') line++;
            Advance();
        }

        if(IsAtEnd())
        {
            Program.Error(line, "Unterminated string.");
            return;
        }

        Advance(); // Consume the closing "

        var value = Source.Substring(start + 1, current - 1 - (start + 1));
        AddToken(TokenType.STRING, value);
    }

    private void Num()
    {
        while(char.IsDigit(Peek())) Advance();

        if(Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance(); // Consume the dot

            while(char.IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.NUMBER, double.Parse(Source.Substring(start, current - start)));
    }

    private void Id()
    {
        while(char.IsLetterOrDigit(Peek()) || Peek() == '_') Advance();

       var text = Source.Substring(start, current - start);
        if(!keywords.TryGetValue(text, out var type)) 
            type = TokenType.IDENTIFIER;

        AddToken(type);
    }
}
