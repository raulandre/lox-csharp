namespace Interpreter;

public class Program
{
    private static bool hadError = false;
    public static void Main(string[] args)
    {
        /*if (args.Length > 1)
        {
            Console.WriteLine("Usage: interpreter [script]");
            Environment.Exit(64);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }*/

        var expr = new Binary(
            new Unary(
                new Token(TokenType.MINUS, "-", null, 1),
                new Literal(123)
            ),
            new Token(TokenType.STAR, "*", null, 1),
            new Grouping(new Literal(45.67))
        );

        Console.WriteLine(new AstPrinter().Print(expr));
    }

    public static void RunFile(string path)
    {
        Run(File.ReadAllText(path));

        if (hadError) Environment.Exit(65);
    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null || line == "quit") break;

            Run(line);
            hadError = false;
        }
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[{line}] Error {where}: {message}");
        hadError = true;
    }
}