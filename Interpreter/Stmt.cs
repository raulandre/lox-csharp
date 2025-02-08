
            // Auto-generated file, do not modify directly.

            namespace Interpreter;

            public interface StmtVisitor<T>
            {
                public T VisitBlockStmt(Block stmt);
public T VisitExpressionStmt(Expression stmt);
public T VisitFunctionStmt(Function stmt);
public T VisitIfStmt(If stmt);
public T VisitWhileStmt(While stmt);
public T VisitReturnStmt(Return stmt);
public T VisitVarStmt(Var stmt);
public T VisitBreakStmt(Break stmt);
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
        

            public class Function : Stmt
            {
                public Token Name { get; private set; }
public List<Token> Parameters { get; private set; }
public List<Stmt> Body { get; private set; }

                public Function(Token name, List<Token> parameters, List<Stmt> body)
                {
                    Name = name;
Parameters = parameters;
Body = body;
                }

public override T Accept<T>(StmtVisitor<T> visitor)
                {
                    return visitor.VisitFunctionStmt(this);
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
        

            public class Return : Stmt
            {
                public Token Keyword { get; private set; }
public Expr Value { get; private set; }

                public Return(Token keyword, Expr value)
                {
                    Keyword = keyword;
Value = value;
                }

public override T Accept<T>(StmtVisitor<T> visitor)
                {
                    return visitor.VisitReturnStmt(this);
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
        

            public class Break : Stmt
            {
                public Token Token { get; private set; }

                public Break(Token token)
                {
                    Token = token;
                }

public override T Accept<T>(StmtVisitor<T> visitor)
                {
                    return visitor.VisitBreakStmt(this);
                }
            }
        
        