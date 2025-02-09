namespace Interpreter;

public enum FunctionType
{
    NONE,
    FUNCTION
}

public class Resolver : ExprVisitor<object>, StmtVisitor<object>
{
    private Interpreter interpreter;
    private Stack<Dictionary<string, bool>> scopes = new();
    private FunctionType currentFunction = FunctionType.NONE;

    public Resolver(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var stmt in statements)
            Resolve(stmt);
    }
    public void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    public void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    public void BeginScope()
    {
        scopes.Push(new());
    }

    public void EndScope()
    {
        scopes.Pop();
    }

    public void Declare(Token name)
    {
        if (scopes.Count == 0)
            return;

        var scope = scopes.Peek();

        if (scope.ContainsKey(name.Lexeme))
        {
            Program.Error(name, $"Variable with name '{name.Lexeme} already exists in this scope.'");
            return;
        }

        scope.Add(name.Lexeme, false);
    }

    public void Define(Token name)
    {
        if (scopes.Count == 0)
            return;

        var scope = scopes.Peek();

        scopes.Peek()[name.Lexeme] = true;
    }

    public void ResolveLocal(Expr expr, Token name)
    {
        for (var i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }

    public void ResolveFunction(Function stmt, FunctionType type)
    {
        var enclosingFunction = currentFunction;
        currentFunction = type;

        BeginScope();
        foreach (var param in stmt.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(stmt.Body);
        EndScope();

        currentFunction = enclosingFunction;
    }

    public object VisitBlockStmt(Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object VisitVarStmt(Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer != null)
            Resolve(stmt.Initializer);
        Define(stmt.Name);
        return null;
    }

    public object VisitVariableExpr(Variable expr)
    {
        if (scopes.Count > 0 && scopes.Peek().ContainsKey(expr.Name.Lexeme) && scopes.Peek()[expr.Name.Lexeme] == false)
            Program.Error(expr.Name, $"Can't read local variable in it's own initializer.");

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object VisitAssignExpr(Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object VisitFunctionStmt(Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public object VisitLambdaExpr(Lambda expr)
    {
        Declare(expr.Function.Name);
        Define(expr.Function.Name);
        ResolveFunction(expr.Function, FunctionType.FUNCTION);
        return null;
    }

    public object VisitExpressionStmt(Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }


    public object VisitIfStmt(If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Thenbranch);
        if (stmt.Elsebranch != null)
            Resolve(stmt.Elsebranch);

        return null;
    }

    public object VisitReturnStmt(Return stmt)
    {
        if (currentFunction != FunctionType.FUNCTION)
            Program.Error(stmt.Keyword, $"Return statement used outside function.");

        if (stmt.Value != null)
            Resolve(stmt.Value);

        return null;
    }

    public object VisitBreakStmt(Break stmt)
    {
        return null;
    }

    public object VisitWhileStmt(While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);

        return null;
    }

    public object VisitBinaryExpr(Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);

        return null;
    }

    public object VisitCallExpr(Call expr)
    {
        if (expr.Callee != null)
            Resolve(expr.Callee);

        expr.Arguments.ForEach(Resolve);

        return null;
    }

    public object VisitGroupingExpr(Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object VisitLiteralExpr(Literal expr)
    {
        return null;
    }

    public object VisitLogicalExpr(Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);

        return null;
    }

    public object VisitUnaryExpr(Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }
}
