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
    "Grouping : Expr expression",
    "Literal : object? value",
    "Unary : Token @operator, Expr right",
    "Variable : Token name"
]);

AST.DefineAST(outputDir, "Stmt",
[
    "Block : List<Stmt?> statements",
    "Expression : Expr expr",
    "Print : Expr expr",
    "Var : Token name, Expr? initializer"
]);

return 0;