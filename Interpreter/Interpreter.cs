namespace Interpreter;

public class Interpreter : ExprVisitor<object>, StmtVisitor<object>
{
    private Env environment = new();

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach(var stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeException ex)
        {
            Program.RuntimeError(ex);
        }
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void ExecuteBlock(List<Stmt> statements, Env environment)
    {
        var previousEnv = this.environment;
        try
        {
            this.environment = environment;

            foreach(var stmt in statements)
                Execute(stmt);

        }
        finally
        {
            this.environment = previousEnv;
        }
    }

    public object VisitWhileStmt(While stmt)
    {
        while(IsTruthy(Eval(stmt.Condition)))
        {
            Execute(stmt.Body);
        }
        return null;
    }

    public object VisitIfStmt(If stmt)
    {
        if(IsTruthy(stmt.Condition))
        {
            Execute(stmt.Thenbranch);
        }
        else if(stmt.Elsebranch != null)
        {
            Execute(stmt.Elsebranch);
        }

        return null;
    }

    public object VisitLogicalExpr(Logical expr)
    {
        var left = Eval(expr.Left);

        // Try to short-circuit if possible
        if(expr.Op.Type == TokenType.OR)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left)) return left;
        }

        return Eval(expr.Right);
    }

    public object VisitBlockStmt(Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Env(environment));
        return null;
    }

    public object VisitAssignExpr(Assign expr)
    {
        var value = Eval(expr.Value);
        environment.Assign(expr.Name, value);
        return value;
    }

    public object VisitVariableExpr(Variable expr)
    {
        return environment.Get(expr.Name);
    }

    public object VisitVarStmt(Var stmt)
    {
        object value = null;
        if(stmt.Initializer != null)
        {
            value = Eval(stmt.Initializer);
        }

        environment.Define(stmt.Name.Lexeme, value);
        return null;
    }

    public object VisitExpressionStmt(Expression stmt)
    {
        var value = Eval(stmt.Expr);
        return null;
    }

    public object VisitPrintStmt(Print stmt)
    {
        var value = Eval(stmt.Expr);
        Console.WriteLine(Stringify(value));
        return null;
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
            case TokenType.PLUS:
                CheckNumberOperand(expr.Op, right);
                return +(double)right;
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
            if (op.Type == TokenType.SLASH)
            {
                if ((double)right != 0) return;

                throw new RuntimeException(op, "Division by zero is not allowed.");
            }

            return;
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
