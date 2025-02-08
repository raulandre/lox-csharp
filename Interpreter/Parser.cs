namespace Interpreter;

public class Parser
{
    private List<Token> tokens;
    private int current = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();

        while (!IsAtEnd())
        {
            statements.Add(Declaration());
        }

        return statements;
    }

    private Stmt Declaration()
    {
        try
        {
            if (Match(TokenType.VAR)) return VarDeclaration();
            return Statement();
        }
        catch (ParseException)
        {
            Synchronize();
            return null;
        }
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expected variable name.");

        Expr initializer = null;
        if (Match(TokenType.EQUAL))
        {
            initializer = Expression();
        }

        Consume(TokenType.SEMICOLON, "Expected ';' after variable declaration.");
        return new Var(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(TokenType.FUN)) return Function("function");
        if (Match(TokenType.FOR)) return ForStmt();
        if (Match(TokenType.IF)) return IfStmt();
        if (Match(TokenType.BREAK)) return BreakStmt();
        if (Match(TokenType.RETURN)) return ReturnStmt();
        if (Match(TokenType.WHILE)) return WhileStmt();
        if (Match(TokenType.LEFT_BRACE)) return new Block(Block());

        return ExpressionStmt();
    }

    private Stmt ReturnStmt()
    {
        var keyword = Previous();
        Expr value = null;

        if (!Check(TokenType.SEMICOLON))
            value = Expression();

        Consume(TokenType.SEMICOLON, "Expected ';' after return value.");
        return new Return(keyword, value);
    }

    private Function Function(string kind)
    {
        Token name = null;
        if (kind != "lambda")
            name = Consume(TokenType.IDENTIFIER, $"Expected {kind} name.");
        else
            Advance();

        Consume(TokenType.LEFT_PAREN, $"Expected '(' after {kind} declaration.");
        var parameters = new List<Token>();

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                    Error(Peek(), "Limit of arguments reached (255).");

                parameters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));
            } while (Match(TokenType.COMMA));
        }

        Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameter list.");
        Consume(TokenType.LEFT_BRACE, $"Expected '{{' before {kind} body.");
        var body = Block();
        return new Function(name, parameters, body);
    }

    private Lambda Lambda()
    {
        var function = Function("lambda");
        return new Lambda(function);
    }

    private Stmt BreakStmt()
    {
        var token = Previous();
        Consume(TokenType.SEMICOLON, "Expected ';' after 'break' statement.");
        return new Break(token);
    }

    private Stmt ForStmt()
    {
        Consume(TokenType.LEFT_PAREN, "Expected '(' after 'for'.");

        Stmt initializer;
        if (Match(TokenType.SEMICOLON))
        {
            initializer = null;
        }
        else if (Match(TokenType.VAR))
        {
            initializer = VarDeclaration();
        }
        else
        {
            initializer = ExpressionStmt();
        }

        Expr condition = null;
        if (!Check(TokenType.SEMICOLON))
        {
            condition = Expression();
        }
        Consume(TokenType.SEMICOLON, "Expected ';' after for loop condition.");

        Expr increment = null;
        if (!Check(TokenType.RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(TokenType.RIGHT_PAREN, "Expected ')' after for loop clauses.");

        var body = Statement();

        if (increment != null)
        {
            body = new Block(new List<Stmt>(){
                    body,
                    new Expression(increment)
                });
        }

        if (condition == null)
            condition = new Literal(true);

        body = new While(condition, body);

        if (initializer != null)
            body = new Block(new List<Stmt>(){
                    initializer,
                    body
                });

        return body;
    }

    private Stmt WhileStmt()
    {
        Consume(TokenType.LEFT_PAREN, "Expected '(' after 'while'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expected ')' after condition.");

        var body = Statement();

        return new While(condition, body);
    }

    private Stmt IfStmt()
    {
        Consume(TokenType.LEFT_PAREN, "Expected '(' after 'if'.");
        var condition = Expression();
        Consume(TokenType.RIGHT_PAREN, "Expected ')' after condition.");

        var thenBranch = Statement();
        Stmt elseBranch = null;
        if (Match(TokenType.ELSE))
        {
            elseBranch = Statement();
        }

        return new If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            statements.Add(Declaration());
        }

        Consume(TokenType.RIGHT_BRACE, "Expected '}' after block.");
        return statements;
    }

    private Stmt ExpressionStmt()
    {
        var expr = Expression();
        Consume(TokenType.SEMICOLON, "Expected ';' after expression.");
        return new Expression(expr);
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Expr Assignment()
    {
        var expr = Or();

        if (Match(TokenType.EQUAL))
        {
            var eq = Previous();
            var value = Assignment();

            if (expr is Variable)
            {
                var name = (expr as Variable)?.Name;
                return new Assign(name, value);
            }

            Error(eq, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(TokenType.OR))
        {
            var op = Previous();
            var right = And();
            expr = new Logical(expr, op, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(TokenType.AND))
        {
            var op = Previous();
            var right = Equality();
            expr = new Logical(expr, op, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();
        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
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
        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
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
        while (Match(TokenType.MINUS, TokenType.PLUS))
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
        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            var op = Previous();
            var right = Unary();
            expr = new Binary(expr, op, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS, TokenType.PLUS))
        {
            var op = Previous();
            var right = Unary();
            return new Unary(op, right);
        }

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(TokenType.LEFT_PAREN))
                expr = FinishCall(expr);
            else
                break;
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();

        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                    Error(Peek(), "Limit of arguments reached (255).");
                arguments.Add(Expression());
            } while (Match(TokenType.COMMA));
        }

        var paren = Consume(TokenType.RIGHT_PAREN, "Expected ')' after argument list.");

        return new Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new Literal(false);
        if (Match(TokenType.TRUE)) return new Literal(true);
        if (Match(TokenType.NIL)) return new Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
        {
            return new Literal(Previous().Literal);
        }

        if (Match(TokenType.IDENTIFIER))
        {
            return new Variable(Previous());
        }

        if (Match(TokenType.LEFT_PAREN))
        {
            var expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
            return new Grouping(expr);
        }

        if (Check(TokenType.FUN))
            return Lambda();

        throw Error(Peek(), "Expected expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private Exception Error(Token token, string message)
    {
        Program.Error(token, message);
        return new ParseException();
    }

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

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd())
        {
            if (Previous().Type == TokenType.SEMICOLON) return;

            switch (Peek().Type)
            {
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.FUN:
                case TokenType.IF:
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
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().Type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens.ElementAt(current);
    }

    private Token Previous()
    {
        return tokens.ElementAt(current - 1);
    }
}
