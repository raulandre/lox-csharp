using Lox.Extensions;
using Lox.Generated;
using Lox.Tokens;
using static Lox.Tokens.TokenType;

namespace Lox.Visitors;

public class Interpreter : Expr.IVisitor<object?>
{
    public void Interpret(Expr expression)
    {
        var value = Evaluate(expression);
        Console.WriteLine(Stringify(value));
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        if (left is null || right is null)
            return null;

        return expr.Operator.Type switch
        {
            MINUS => CheckNumberOperands(expr.Operator, left, right).Subtract(),
            SLASH => CheckNumberOperands(expr.Operator, left, right).Divide(),
            STAR => CheckNumberOperands(expr.Operator, left, right).Multiply(),
            PLUS => AddOrConcatenate(expr.Operator, left, right),
            GREATER => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l > r),
            GREATER_EQUAL => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l >= r),
            LESS => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l < r),
            LESS_EQUAL => CheckNumberOperands(expr.Operator, left, right).Apply((l, r) => l <= r),
            BANG_EQUAL => !IsEqual(left, right),
            EQUAL_EQUAL => IsEqual(left, right),
            _ => null
        };
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.Expression);
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);
        if (right is null) return right;

        return expr.Operator.Type switch
        {
            MINUS => -CheckNumberOperand(expr.Operator, right),
            BANG => !IsTruthy(right),
            _ => null,
        };
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private static bool IsTruthy(object obj)
    {
        if (obj is null) return false;
        if (obj is bool v) return v;
        if (obj is double d) return d != 0.0;

        return true;
    }

    private static bool IsEqual(object left, object right)
    {
        if (left is null && right is null) return true;
        if (left is null) return false;

        return left == right;
    }

    private static object? AddOrConcatenate(Token @operator, params object[] objects)
        => objects switch
        {
        [double l, double r] => l + r,
        [string l, string r] => l + r,
            _ => throw new RuntimeError(@operator, "Operands must be two numbers or two strings.")
        };

    private static double CheckNumberOperand(Token @operator, object operand)
    {
        if (operand is double number) return number;
        throw new RuntimeError(@operator, "Operand must be a number.");
    }

    private static (double, double) CheckNumberOperands(
        Token @operator,
        object left,
        object right
    )
    {
        if (left is double n1 && right is double n2) return (n1, n2);
        throw new RuntimeError(@operator, "Operands must be numbers.");
    }

    private static string? Stringify(object? obj)
    {
        return obj switch
        {
            null => "nil",
            _ => obj.ToString()
        };
    }
}