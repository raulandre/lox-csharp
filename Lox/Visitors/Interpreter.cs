using System.Globalization;
using Lox.Extensions;
using Lox.Generated;
using Lox.Functions;
using Lox.Tokens;
using static Lox.Tokens.TokenType;

namespace Lox.Visitors;

public class Interpreter : Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    public static Environment Globals { get; private set; } = new();
    private Environment Environment = Globals;

    public Interpreter()
    {
        Globals.Define("clock", new Clock());
    }

    public void Interpret(List<Stmt?> statements)
    {
        try
        {
            foreach (var stmt in statements)
            {
                if (stmt is not null)
                    Execute(stmt);
            }
        }
        catch (RuntimeError error)
        {
            Program.RuntimeError(error);
        }
    }

    private void Execute(Stmt? statement)
        => statement?.Accept(this);

    public void ExecuteBlock(List<Stmt?> statements, Environment environment)
    {
        var previousEnv = Environment;
        try
        {
            Environment = environment;

            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            Environment = previousEnv;
        }
    }

    public object? VisitBlockStmt(Stmt.Block block)
    {
        ExecuteBlock(block.Statements, new Environment(Environment));
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        return expr.Operator.Type switch
        {
            MINUS => CheckNumberOperands(expr.Operator, left, right).Subtract(),
            STAR => CheckNumberOperands(expr.Operator, left, right).Multiply(),
            PLUS => AddOrConcatenate(expr.Operator, left, right),
            SLASH => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => CheckedDivision(expr.Operator, l, r)),
            GREATER => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l > r),
            GREATER_EQUAL => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l >= r),
            LESS => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l < r),
            LESS_EQUAL => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l <= r),
            BANG_EQUAL => !IsEqual(left, right),
            EQUAL_EQUAL => IsEqual(left, right),
            _ => null
        };
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        var callee = Evaluate(expr.Callee);

        if (callee is not ICallable)
            throw new RuntimeError(expr.Paren, "Calling non-callable object.");

        var args = expr.Arguments.Select(Evaluate);

        var function = callee as ICallable;

        if (args.Count() != function!.Arity())
            throw new RuntimeError(expr.Paren, $"Expected {function.Arity()} arguments in function call, got {args.Count()}.");

        return function!.Call(this, args);
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Operator.Type == OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Evaluate(expr.Right);
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        if (right is null) return right;

        return expr.Operator.Type switch
        {
            MINUS => CheckNumberOperand(expr.Operator, right) * -1.0,
            BANG => !IsTruthy(right),
            _ => null,
        };
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return Environment.Get(expr.Name);
    }

    private object? Evaluate(Expr? expr)
        => expr?.Accept(this);

    private static bool IsTruthy(object? obj)
        => obj switch
        {
            bool b => b,
            double d => d != 0,
            null => false,
            _ => true,
        };

    private static bool IsEqual(object? left, object? right)
    {
        if (left is null && right is null) return true;
        if (left is null) return false;

        return left.Equals(right);
    }

    private static object? AddOrConcatenate(Token @operator, object? left, object? right)
    {
        var objects = new[] { left, right };
        return objects switch
        {
        [double l, double r] => l + r,
        [string l, string r] => l + r,
        [string s, object o] => s + Stringify(o),
        [object o, string s] => Stringify(o) + s,
        [null, string s] => Stringify(null) + s,
        [string s, null] => s + Stringify(null),
            _ => throw new RuntimeError(@operator, "Invalid operands for '+' operator.")
        };
    }

    private static double CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double number) return number;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private static (double, double) CheckNumberOperands(
        Token @operator,
        object? left,
        object? right
    )
    {
        if (left is double n1 && right is double n2) return (n1, n2);
        throw new RuntimeError(@operator, "Operands must be numbers.");
    }

    private static double CheckedDivision(Token @operator, double a, double b)
    {
        if (b == 0)
            throw new RuntimeError(@operator, "Division by zero detected.");

        return a / b;
    }

    private static string? Stringify(object? obj)
    {
        var cultureInfo = new CultureInfo("en-US");
        return obj switch
        {
            double d => Math.Truncate(d) == d
                ? Math.Truncate(d).ToString(cultureInfo)
                : d.ToString(cultureInfo),
            null => "nil",
            _ => obj.ToString()
        };
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        var function = new LoxFunction(stmt);
        Environment.Define(stmt.Name.Lexeme, function);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
            Execute(stmt.ThenBranch);
        else if (stmt.ElseBranch is not null)
            Execute(stmt.ElseBranch);

        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        var value = Evaluate(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        object? value = null;
        if (stmt.Value is not null)
            value = Evaluate(stmt.Value);

        throw new Return(value);
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.Initializer is not null)
            value = Evaluate(stmt.Initializer);

        Environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.Body);
        }

        return null;
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);
        Environment.Assign(expr.Name, value);
        return value;
    }
}