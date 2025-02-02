// Auto-generated file, do not modify directly.

namespace Interpreter;

public interface StmtVisitor<T>
{
    public T VisitBlockStmt(Block stmt);
    public T VisitExpressionStmt(Expression stmt);
    public T VisitIfStmt(If stmt);
    public T VisitPrintStmt(Print stmt);
    public T VisitWhileStmt(While stmt);
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


public class If : Stmt
{
    public Expr Condition { get; private set; }
    public Stmt Thenbranch { get; private set; }
    public Stmt Elsebranch { get; private set; }

    public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
    {
        Condition = condition;
        Thenbranch = thenBranch;
        Elsebranch = elseBranch;
    }

    public override T Accept<T>(StmtVisitor<T> visitor)
    {
        return visitor.VisitIfStmt(this);
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


public class While : Stmt
{
    public Expr Condition { get; private set; }
    public Stmt Body { get; private set; }

    public While(Expr condition, Stmt body)
    {
        Condition = condition;
        Body = body;
    }

    public override T Accept<T>(StmtVisitor<T> visitor)
    {
        return visitor.VisitWhileStmt(this);
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
