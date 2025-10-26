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
            if (Match(CLASS)) return ClassDeclaration();
            if (Match(FUN)) return Function("function");
            if (Match(VAR)) return VarDeclaration();
            return Statement();
        }
        catch (ParserError)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt ClassDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expected class name.");

        Expr.Variable? superclass = null;
        if (Match(LESS))
        {
            Consume(IDENTIFIER, "Expected superclass name.");
            superclass = new Expr.Variable(Previous());
        }

        Consume(LEFT_BRACE, "Expected '{' after class name.");

        var methods = new List<Stmt.Function>();
        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add(Function("method"));
        }

        Consume(RIGHT_BRACE, "Expected '}' after class body.");
        return new Stmt.Class(name, methods, superclass);
    }

    private Stmt.Function Function(string kind)
    {
        var name = Consume(IDENTIFIER, $"Expected {kind} name.");
        Consume(LEFT_PAREN, $"Expected '(' after {kind} name.");

        var @params = new List<Token>();
        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (@params.Count >= 255)
                    Error(Peek(), "Parameter limit exceeded (255).");
                @params.Add(Consume(IDENTIFIER, "Expected parameter name."));
            } while (Match(COMMA));
        }

        Consume(RIGHT_PAREN, $"Expected ')' after {kind} name.");
        Consume(LEFT_BRACE, $"Expected '{{' before {kind} body.");
        var body = Block();

        return new Stmt.Function(name, @params, body);
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
        if (Match(FOR)) return ForStatement();
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(RETURN)) return ReturnStatement();
        if (Match(WHILE)) return WhileStatement();
        if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt.Return ReturnStatement()
    {
        var keyword = Previous();

        Expr? value = null;
        if (!Check(SEMICOLON))
            value = Expression();

        Consume(SEMICOLON, "Expected ';' after return statement.");
        return new Stmt.Return(keyword, value);
    }

    private Stmt ForStatement()
    {
        Consume(LEFT_PAREN, "Expected '(' after 'for'.");

        Stmt? initializer;
        Expr? condition = null;
        Expr? increment = null;

        if (Match(SEMICOLON))
            initializer = null;
        else if (Match(VAR))
            initializer = VarDeclaration();
        else
            initializer = ExpressionStatement();

        if (!Check(SEMICOLON))
            condition = Expression();
        Consume(SEMICOLON, "Expected ';' after condition.");

        if (!Check(RIGHT_PAREN))
            increment = Expression();
        Consume(RIGHT_PAREN, "Expected ')' after 'for'.");

        var body = Statement();

        if (increment is not null)
        {
            body = new Stmt.Block(
            [
                body,
                new Stmt.Expression(increment)
            ]);
        }

        condition ??= new Expr.Literal(true);
        body = new Stmt.While(condition, body);

        if (initializer is not null)
            body = new Stmt.Block([
                initializer, body
            ]);

        return body;
    }

    private Stmt.While WhileStatement()
    {
        Consume(LEFT_PAREN, "Expected '(' after 'while'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expected ')' after condition.");

        var body = Statement();
        return new Stmt.While(condition, body);
    }

    private Stmt.If IfStatement()
    {
        Consume(LEFT_PAREN, "Expected '(' after 'if'.");
        var condition = Expression();
        Consume(RIGHT_PAREN, "Expected ')' after condition.");

        var thenBranch = Statement();
        var elseBranch = Match(ELSE) ? Statement() : null;

        return new Stmt.If(condition, thenBranch, elseBranch);
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
        var expr = Or();

        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable variable)
            {
                var name = variable.Name;
                return new Expr.Assign(name, value);
            }
            else if (expr is Expr.Get)
            {
                var get = expr as Expr.Get;
                return new Expr.Set(get!.Object, get.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var @operator = Previous();
            var right = And();
            expr = new Expr.Logical(expr, @operator, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var @operator = Previous();
            var right = Equality();
            expr = new Expr.Logical(expr, @operator, right);
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

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
                expr = FinishCall(expr);
            else if (Match(DOT))
            {
                var name = Consume(IDENTIFIER, "Expected property name after '.'");
                expr = new Expr.Get(expr, name);
            }
            else
                break;
        }

        return expr;
    }

    private Expr.Call FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();

        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                    Error(Peek(), "Argument limit exceeded (255).");
                arguments.Add(Expression());
            } while (Match(COMMA));
        }

        var paren = Consume(RIGHT_PAREN, "Expected ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
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

        if (Match(SUPER))
        {
            var keyword = Previous();
            Consume(DOT, "Expected '.' after 'super'.");
            var method = Consume(IDENTIFIER, "Expect superclass method name.");
            return new Expr.Super(keyword, method);
        }

        if (Match(THIS))
            return new Expr.This(Previous());

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