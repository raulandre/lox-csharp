using System.Globalization;

namespace GenerateAst;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: generate_ast <output_dir>");
            Environment.Exit(64);
        }

        var outputDir = args.First();

        DefineAst(outputDir, "Expr", new List<string>() {
            "Assign: Token name, Expr value",
            "Binary: Expr left, Token op, Expr right",
            "Grouping: Expr expression",
            "Literal: object value",
            "Logical: Expr left, Token op, Expr right",
            "Unary: Token op, Expr right",
            "Variable: Token name"
        });

        DefineAst(outputDir, "Stmt", new List<string>() {
            "Block: List<Stmt> statements",
            "Expression: Expr expr",
            "If: Expr condition, Stmt thenBranch, Stmt elseBranch",
            "Print: Expr expr",
            "While: Expr condition, Stmt body",
            "Var: Token name, Expr initializer"
        });
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        var path = $"{outputDir}/{baseName}.cs";
        var definedTypes = types.Select(type =>
        {
            var typeData = type.Split(":");
            var className = typeData.First().Trim();
            var fields = typeData.Skip(1).First().Trim();

            return DefineType(baseName, className, fields);
        });

        var fileContent = $@"
            // Auto-generated file, do not modify directly.

            namespace Interpreter;

            public interface {baseName}Visitor<T>
            {{
                {string.Join('\n',
                    types.Select(type =>
                    {
                        var typeName = type.Split(":")[0].Trim();
                        return $"public T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});";
                    }))}
            }}

            public abstract class {baseName}
            {{
                    public abstract T Accept<T>({baseName}Visitor<T> visitor);
            }};

            {string.Join('\n', definedTypes)}
        ";

        File.WriteAllText(path, fileContent);
    }

    private static string DefineType(string baseName, string className, string fields)
        => $@"
            public class {className} : {baseName}
            {{
                {string.Join('\n',
                    fields.Split(", ").Select(field =>
                    $"public {field.Split(" ")[0]} {CultureInfo.CurrentCulture.TextInfo.ToTitleCase(field.Split(" ")[1])} {{ get; private set; }}")
                    )}

                public {className}({fields})
                {{
                    {string.Join('\n', fields.Split(", ").Select(field =>
                        $"{CultureInfo.CurrentCulture.TextInfo.ToTitleCase(field.Split(" ")[1])} = {field.Split(" ")[1]};"))}
                }}

public override T Accept<T>({baseName}Visitor<T> visitor)
                {{
                    return visitor.Visit{className}{baseName}(this);
                }}
            }}
        ";
}
