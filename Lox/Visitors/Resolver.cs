using System.Diagnostics.Tracing;
using System.Text;
using Lox.Generated;
using Lox.Tokens;

namespace Lox.Visitors;

public class Resolver(Interpreter interpreter) : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    public Interpreter Interpreter { get; private set; } = interpreter;
    public Stack<Dictionary<string, bool>> Scopes { get; private set; } = new();
    public FunctionType CurrentFunction { get; private set; } = FunctionType.NONE;
    public ClassType CurrentClass { get; private set; } = ClassType.NONE;

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);
        if (stmt.Initializer is not null)
        {
            Resolve(stmt.Initializer);
        }
        Define(stmt.Name);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (Scopes.Count > 0 && Scopes.Peek().ContainsKey(expr.Name.Lexeme) && Scopes.Peek()[expr.Name.Lexeme] == false)
            Program.Error(expr.Name, "Can't read variable in its own initializer.");

        ResolveLocal(expr, expr.Name);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);
        ResolveFunction(stmt, FunctionType.FUNCTION);
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);

        if (stmt.ElseBranch is not null)
            Resolve(stmt.ElseBranch);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (CurrentFunction == FunctionType.NONE)
        {
            Program.Error(stmt.Keyword, "Invalid usage of 'return' outside function scope.");
        }

        if (stmt.Value is not null)
        {
            if (CurrentFunction == FunctionType.INITIALIZER)
            {
                Program.Error(stmt.Keyword, "Invalid usage of 'return' inside initializer.");
            }
            Resolve(stmt.Value);
        }
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);
        foreach (var arg in expr.Arguments)
            Resolve(arg);
        return null;
    }

    public object? VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);
        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
        return null;
    }

    public object? VisitThisExpr(Expr.This expr)
    {
        if(CurrentClass == ClassType.NONE)
        {
            Program.Error(expr.Keyword, "Invalid usage of 'this' outside class.");
            return null;
        }
        ResolveLocal(expr, expr.Keyword);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Left);
        Resolve(expr.Right);
        return null;
    }

    public object? VisitClassStmt(Stmt.Class stmt)
    {
        var enclosingClass = CurrentClass;
        CurrentClass = ClassType.CLASS;
        Declare(stmt.Name);
        Define(stmt.Name);

        BeginScope();
        Scopes.Peek().Add("this", true);

        foreach (var method in stmt.Methods)
        {
            var declaration = FunctionType.METHOD;

            if (method.Name.Lexeme.Equals("init"))
                declaration = FunctionType.INITIALIZER;

            ResolveFunction(method, declaration);
        }

        EndScope();
        CurrentClass = enclosingClass;
        return null;
    }

    #region Helpers

    public void Resolve(List<Stmt?> statements)
    {
        foreach (var stmt in statements)
            Resolve(stmt);
    }

    public void Resolve(Stmt? statement)
    {
        statement?.Accept(this);
    }

    public void Resolve(Expr? expr)
    {
        expr?.Accept(this);
    }

    public void BeginScope()
    {
        Scopes.Push([]);
    }

    public void EndScope()
    {
        Scopes.Pop();
    }

    public void Declare(Token name)
    {
        if (Scopes.Count == 0)
            return;

        var scope = Scopes.Peek();

        if (scope.ContainsKey(name.Lexeme))
            Program.Error(name, $"Redeclaration of local variable '{name.Lexeme}'.");

        scope[name.Lexeme] = false;
    }

    public void Define(Token name)
    {
        if (Scopes.Count == 0)
            return;

        Scopes.Peek()[name.Lexeme] = true;
    }

    public void ResolveLocal(Expr expr, Token name)
    {
        for (var i = Scopes.Count - 1; i >= 0; i--)
        {
            /*
            Note to self: C# stacks are represented as lists 
            where the first element is the TOP of the stack
            not the bottom. Hence the Reverse() here.
            */
            if (Scopes.Reverse().ElementAt(i).ContainsKey(name.Lexeme))
            {
                Interpreter.Resolve(expr, Scopes.Count - 1 - i);
                return;
            }
        }
    }

    public void ResolveFunction(Stmt.Function function, FunctionType functionType)
    {
        var enclosingFunction = CurrentFunction;
        CurrentFunction = functionType;
        BeginScope();
        foreach (var param in function.Params)
        {
            Declare(param);
            Define(param);
        }
        Resolve(function.Body);
        EndScope();
        CurrentFunction = enclosingFunction;
    }

    #endregion
}

public enum FunctionType
{
    NONE = 0,
    FUNCTION,
    METHOD,
    INITIALIZER
}

public enum ClassType
{
    NONE = 0,
    CLASS
}