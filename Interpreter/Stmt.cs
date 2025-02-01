
// Auto-generated file, do not modify directly.

namespace Interpreter;

public interface StmtVisitor<T>
{
    public T VisitBlockStmt(Block stmt);
    public T VisitExpressionStmt(Expression stmt);
    public T VisitPrintStmt(Print stmt);
    public T VisitVarStmt(Var stmt);
}

public abstract class Stmt
{
    public abstract T Accept<T>(StmtVisitor<T> visitor);
};

            
public class Block : Stmt
{
    public List<Stmt> Statements { get; private set; }

    public Block(List<Stmt> statements)
    {
        Statements = statements;
    }

    public override T Accept<T>(StmtVisitor<T> visitor)
    {
        return visitor.VisitBlockStmt(this);
    }
}
        

public class Expression : Stmt
{
    public Expr Expr { get; private set; }

    public Expression(Expr expr)
    {
        Expr = expr;
    }

    public override T Accept<T>(StmtVisitor<T> visitor)
    {
        return visitor.VisitExpressionStmt(this);
    }
}
        

public class Print : Stmt
{
    public Expr Expr { get; private set; }

    public Print(Expr expr)
    {
        Expr = expr;
    }

    public override T Accept<T>(StmtVisitor<T> visitor)
    {
        return visitor.VisitPrintStmt(this);
    }
}
        

public class Var : Stmt
{
    public Token Name { get; private set; }
    public Expr Initializer { get; private set; }

    public Var(Token name, Expr initializer)
    {
        Name = name;
        Initializer = initializer;
    }

    public override T Accept<T>(StmtVisitor<T> visitor)
    {
        return visitor.VisitVarStmt(this);
    }
}
