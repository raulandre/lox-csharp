// Auto-generated file, do not modify directly.

namespace Interpreter;

public interface Visitor<T>
{
    public T VisitBinaryExpr(Binary expr);
    public T VisitGroupingExpr(Grouping expr);
    public T VisitLiteralExpr(Literal expr);
    public T VisitUnaryExpr(Unary expr);
}

public abstract class Expr
{
    public abstract T Accept<T>(Visitor<T> visitor);
};


public class Binary : Expr
{
    public Expr Left { get; private set; }
    public Token Op { get; private set; }
    public Expr Right { get; private set; }

    public Binary(Expr left, Token op, Expr right)
    {
        Left = left;
        Op = op;
        Right = right;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitBinaryExpr(this);
    }
}


public class Grouping : Expr
{
    public Expr Expression { get; private set; }

    public Grouping(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitGroupingExpr(this);
    }
}


public class Literal : Expr
{
    public object Value { get; private set; }

    public Literal(object value)
    {
        Value = value;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitLiteralExpr(this);
    }
}


public class Unary : Expr
{
    public Token Op { get; private set; }
    public Expr Right { get; private set; }

    public Unary(Token op, Expr right)
    {
        Op = op;
        Right = right;
    }

    public override T Accept<T>(Visitor<T> visitor)
    {
        return visitor.VisitUnaryExpr(this);
    }
}

