namespace Lox;

using Lox.Generated;
using Lox.Tokens;
using static Lox.Tokens.TokenType;

public class Parser(List<Token> tokens)
{
    private readonly List<Token> Tokens = tokens;
    private int Current = 0;

    public List<Stmt?>? Parse()
    {
        List<Stmt?> statements = [];
        while (!IsAtEnd())
            statements.Add(Declaration());
        return statements;
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(VAR)) return VarDeclaration();
            return Statement();
        }
        catch (ParserError)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt.Var VarDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expected variable name.");

        Expr? initializer = null;
        if (Match(EQUAL))
            initializer = Expression();

        Consume(SEMICOLON, "Expected ';' after variable initialization");
        return new Stmt.Var(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(PRINT)) return PrintStatement();
        if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private List<Stmt?> Block()
    {
        var statements = new List<Stmt?>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(RIGHT_BRACE, "Expected '}' after block.");

        return statements;
    }

    private Stmt.Print PrintStatement()
    {
        var value = Expression();
        Consume(SEMICOLON, "Expected ';' after value.");
        return new Stmt.Print(value);
    }

    private Stmt.Expression ExpressionStatement()
    {
        var expr = Expression();
        Consume(SEMICOLON, "Expected ';' after expression.");
        return new Stmt.Expression(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Equality();

        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                var name = variable.Name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();

        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var @operator = Previous();
            var right = Comparison();

            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var @operator = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(MINUS, PLUS))
        {
            var @operator = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(STAR, SLASH))
        {
            var @operator = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, @operator, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            var @operator = Previous();
            var right = Unary();
            return new Expr.Unary(@operator, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();
            Consume(RIGHT_PAREN, "Expect ')' after expression.");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expected expression.");
    }

    #region Helper methods
    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) Current++;
        return Previous();
    }

    private bool IsAtEnd()
        => Peek().Type == EOF;

    private Token Peek()
        => Tokens.ElementAt(Current);

    private Token Previous()
        => Tokens.ElementAt(Current - 1);

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }
    #endregion

    #region Error handling
    private static ParserError Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParserError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON) return;

            switch (Peek().Type)
            {
                case CLASS:
                case FOR:
                case FUN:
                case IF:
                case PRINT:
                case RETURN:
                case VAR:
                case WHILE:
                    return;
            }

            Advance();
        }
    }
    #endregion
}

public class ParserError : Exception
{ }