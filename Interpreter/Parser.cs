namespace Interpreter;

public class Parser
{
    private List<Token> Tokens;
    private int Current = 0;

    public Parser(List<Token> tokens)
    {
        Tokens = tokens;
    }

    public Expr Parse()
    {
        try
        {
            return Expression();
        }
        catch(ParseException)
        {
            return null;
        }
    }

    private Expr Expression()
    {
        return Equality();
    }

    private Expr Equality()
    {
        var expr = Comparison();
        while(Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            var op = Previous();
            var right = Comparison();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();
        while(Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            var op = Previous();
            var right = Term();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();
        while(Match(TokenType.MINUS, TokenType.PLUS))
        {
            var op = Previous();
            var right = Factor();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();
        while(Match(TokenType.SLASH, TokenType.STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if(Match(TokenType.BANG, TokenType.MINUS))
        {
            var op = Previous();
            var right = Unary();
            return new Unary(op, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if(Match(TokenType.FALSE)) return new Literal(false);
        if(Match(TokenType.TRUE)) return new Literal(true);
        if(Match(TokenType.NIL)) return new Literal(null);

        if(Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Literal(Previous().Literal);
        }

        if(Match(TokenType.LEFT_PAREN))
        {
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
            return new Grouping(expr);
        }

        throw Error(Peek(), "Expected expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if(Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private Exception Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseException();
    }

    private bool Match(params TokenType[] types)
    {
        foreach(var type in types)
        {
            if(Check(type)) {
                Advance();
                return true;
            }
        }

        return false;
    }

    private void Synchronize()
    {
        Advance();
        while(!IsAtEnd())
        {
            if(Previous().Type == TokenType.SEMICOLON) return;

            switch(Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.FUN:
                case TokenType.IF:
                case TokenType.PRINT:
                case TokenType.RETURN:
                case TokenType.VAR:
                case TokenType.WHILE:
                    return;
            }

            Advance();
        }
    }

    private bool Check(TokenType type)
    {
        if(IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if(!IsAtEnd()) Current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return Tokens.ElementAt(Current);
    }

    private Token Previous()
    {
        return Tokens.ElementAt(Current - 1);
    }
}
