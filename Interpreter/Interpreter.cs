namespace Interpreter;

public class Interpreter : Visitor<object>
{
    public void Interpret(Expr expr)
    {
        try
        {
            var value = Eval(expr);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeException ex)
        {
            Program.RuntimeError(ex);
        }
    }

    public object VisitLiteralExpr(Literal expr)
    {
        return expr.Value;
    }

    public object VisitGroupingExpr(Grouping expr)
    {
        return Eval(expr.Expression);
    }

    public object VisitUnaryExpr(Unary expr)
    {
        var right = Eval(expr.Right);

        switch (expr.Op.Type)
        {
            case TokenType.BANG:
                return !IsTruthy(right);
            case TokenType.MINUS:
                CheckNumberOperand(expr.Op, right);
                return -(double)right;
        }

        return null;
    }

    public object VisitBinaryExpr(Binary expr)
    {
        var left = Eval(expr.Left);
        var right = Eval(expr.Right);

        switch (expr.Op.Type)
        {
            case TokenType.MINUS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left - (double)right;
            case TokenType.SLASH:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left / (double)right;
            case TokenType.STAR:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left * (double)right;
            case TokenType.PLUS:
                if (left is double && right is double)
                {
                    return (double)left + (double)right;
                }
                else if (left is string && right is string)
                {
                    return (string)left + (string)right;
                }
                else if (left is string)
                {
                    return (string)left + right.ToString();
                }
                else if (right is string)
                {
                    return left.ToString() + (string)right;
                }

                throw new RuntimeException(expr.Op, "Operands must be two numbers or two strings.");

            case TokenType.GREATER:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left > (double)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left >= (double)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left < (double)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.Op, left, right);
                return (double)left <= (double)right;
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
        }

        return null;
    }

    private object Eval(Expr expr)
    {
        return expr.Accept(this);
    }

    private bool IsTruthy(object obj)
    {
        if (obj is null) return false;
        if (obj is bool) return (bool)obj;
        if (obj is double && (double)obj == 0) return false;

        return true;
    }

    private bool IsEqual(object left, object right)
    {
        if (left is null && right is null) return true;
        if (left is null) return false;

        return left.Equals(right);
    }

    private void CheckNumberOperand(Token op, object operand)
    {
        if (operand is double) return;
        throw new RuntimeException(op, "Operand must be a number.");
    }

    private void CheckNumberOperands(Token op, object left, object right)
    {
        if (left is double && right is double)
        {
            // Should we use an epsilon here?
            if((double)right != 0) return;

            throw new RuntimeException(op, "Division by zero is not allowed.");
        }
        throw new RuntimeException(op, "Operands must be numbers.");
    }

    private string Stringify(object obj)
    {
        if (obj is null) return "nil";

        if (obj is double)
        {
            var text = obj.ToString();
            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return obj.ToString();
    }
}
