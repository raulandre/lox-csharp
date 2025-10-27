using ASTGen;

if (args.Length != 1)
{
    Console.WriteLine("Usage: ASTGen <output_dir>");
    return 1;
}

var outputDir = args[0];
AST.DefineAST(outputDir, "Expr",
[
    "Assign : Token name, Expr value",
    "Binary : Expr left, Token @operator, Expr right",
    "Call : Expr callee, Token paren, List<Expr> arguments",
    "Get : Expr @object, Token name",
    "Grouping : Expr expression",
    "Literal : object? value",
    "Logical : Expr left, Token @operator, Expr right",
    "Set : Expr @object, Token name, Expr? value",
    "Super : Token keyword, Token method",
    "This: Token keyword",
    "Unary : Token @operator, Expr right",
    "Pipe : Token @operator, Expr left, Expr right",
    "Variable : Token name"
]);

AST.DefineAST(outputDir, "Stmt",
[
    "Block : List<Stmt?> statements",
    "Class : Token name, List<Stmt.Function> methods, Expr.Variable? superclass",
    "Expression : Expr expr",
    "Function : Token name, List<Token> @params, List<Stmt?> body",
    "If: Expr condition, Stmt thenBranch, Stmt? elseBranch",
    "Print : Expr expr",
    "Return : Token keyword, Expr? value",
    "While : Expr condition, Stmt body",
    "Var : Token name, Expr? initializer"
]);

return 0;