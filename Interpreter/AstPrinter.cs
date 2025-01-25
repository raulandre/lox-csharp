namespace Interpreter;

public class AstPrinter : Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this);
    }

    public string VisitBinaryExpr(Binary expr)
    {
        return Parenthesize(expr.Op.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Literal expr)
    {
        if(expr.Value == null) return "nil";
        return expr.Value.ToString();
    }

    public string VisitUnaryExpr(Unary expr)
    {
        return Parenthesize(expr.Op.Lexeme, expr.Right);
    }

    private string Parenthesize(string name, params Expr[] exprs)
    {
        var content = string.Join(" ", exprs.ToList().Select(expr => expr.Accept(this)));
        return $"({name} {content})";
    }
}
