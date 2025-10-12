using ASTGen;

if (args.Length != 1)
{
    Console.WriteLine("Usage: ASTGen <output_dir>");
    return 1;
}

var outputDir = args[0];
AST.DefineAST(outputDir, "Expr",
[
    "Binary : Expr left, Token @operator, Expr right",
    "Grouping : Expr expression",
    "Literal : object? value",
    "Unary : Token @operator, Expr right"
]);

return 0;