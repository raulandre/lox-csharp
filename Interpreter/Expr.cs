
            // Auto-generated file, do not modify directly.

            namespace Interpreter;

            public interface ExprVisitor<T>
            {
                public T VisitAssignExpr(Assign expr);
public T VisitBinaryExpr(Binary expr);
public T VisitGroupingExpr(Grouping expr);
public T VisitLiteralExpr(Literal expr);
public T VisitUnaryExpr(Unary expr);
public T VisitVariableExpr(Variable expr);
            }

            public abstract class Expr
            {
                    public abstract T Accept<T>(ExprVisitor<T> visitor);
            };

            
            public class Assign : Expr
            {
                public Token Name { get; private set; }
public Expr Value { get; private set; }

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
                public Expr Left { get; private set; }
public Token Op { get; private set; }
public Expr Right { get; private set; }

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
        

            public class Grouping : Expr
            {
                public Expr Expression { get; private set; }

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
                public object Value { get; private set; }

                public Literal(object value)
                {
                    Value = value;
                }

public override T Accept<T>(ExprVisitor<T> visitor)
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

public override T Accept<T>(ExprVisitor<T> visitor)
                {
                    return visitor.VisitUnaryExpr(this);
                }
            }
        

            public class Variable : Expr
            {
                public Token Name { get; private set; }

                public Variable(Token name)
                {
                    Name = name;
                }

public override T Accept<T>(ExprVisitor<T> visitor)
                {
                    return visitor.VisitVariableExpr(this);
                }
            }
        
        