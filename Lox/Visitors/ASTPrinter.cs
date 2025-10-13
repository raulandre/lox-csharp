namespace Lox.Visitors;

using System.Text;
using Lox.Generated;
using static Lox.Generated.Expr;

public class ASTPrinter : IVisitor<string?>
{
    public string? Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string? VisitBinaryExpr(Binary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
    }

    public string? VisitGroupingExpr(Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string? VisitLiteralExpr(Literal expr)
    {
        if (expr.Value is null) return "nil";
        return expr.Value.ToString();
    }

    public string? VisitUnaryExpr(Unary expr)
    {
        return Parenthesize(expr.Operator.Lexeme, expr.Right);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var sb = new StringBuilder();

        sb.Append($"({name}");
        foreach (var expr in exprs)
        {
            sb.Append(' ');
            sb.Append(expr.Accept(this));
        }
        sb.Append(')');

        return sb.ToString();
    }
}