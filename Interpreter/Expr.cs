
// Auto-generated file, do not modify directly.

namespace Interpreter;

            public interface ExprVisitor<T>
{
    public T VisitAssignExpr(Assign expr);
    public T VisitBinaryExpr(Binary expr);
    public T VisitCallExpr(Call expr);
    public T VisitGroupingExpr(Grouping expr);
    public T VisitLiteralExpr(Literal expr);
    public T VisitLogicalExpr(Logical expr);
    public T VisitUnaryExpr(Unary expr);
    public T VisitVariableExpr(Variable expr);
    public T VisitLambdaExpr(Lambda expr);
}

public abstract class Expr
{
    public abstract T Accept<T>(ExprVisitor<T> visitor);
};


public class Assign : Expr
{
    public Token Name { get; set; }
    public Expr Value { get; set; }

    public Assign(Token name, Expr value)
    {
        Name = name;
        Value = value;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitAssignExpr(this);
    }
}


public class Binary : Expr
{
    public Expr Left { get; set; }
    public Token Op { get; set; }
    public Expr Right { get; set; }

    public Binary(Expr left, Token op, Expr right)
    {
        Left = left;
        Op = op;
        Right = right;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitBinaryExpr(this);
    }
}


public class Call : Expr
{
    public Expr Callee { get; set; }
    public Token Paren { get; set; }
    public List<Expr> Arguments { get; set; }

    public Call(Expr callee, Token paren, List<Expr> arguments)
    {
        Callee = callee;
        Paren = paren;
        Arguments = arguments;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitCallExpr(this);
    }
}


public class Grouping : Expr
{
    public Expr Expression { get; set; }

    public Grouping(Expr expression)
    {
        Expression = expression;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitGroupingExpr(this);
    }
}


public class Literal : Expr
{
    public object Value { get; set; }

    public Literal(object value)
    {
        Value = value;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitLiteralExpr(this);
    }
}


public class Logical : Expr
{
    public Expr Left { get; set; }
    public Token Op { get; set; }
    public Expr Right { get; set; }

    public Logical(Expr left, Token op, Expr right)
    {
        Left = left;
        Op = op;
        Right = right;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitLogicalExpr(this);
    }
}


public class Unary : Expr
{
    public Token Op { get; set; }
    public Expr Right { get; set; }

    public Unary(Token op, Expr right)
    {
        Op = op;
        Right = right;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitUnaryExpr(this);
    }
}


public class Variable : Expr
{
    public Token Name { get; set; }

    public Variable(Token name)
    {
        Name = name;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitVariableExpr(this);
    }
}


public class Lambda : Expr
{
    public Function Function { get; set; }

    public Lambda(Function function)
    {
        Function = function;
    }

    public override T Accept<T>(ExprVisitor<T> visitor)
    {
        return visitor.VisitLambdaExpr(this);
    }
}


        