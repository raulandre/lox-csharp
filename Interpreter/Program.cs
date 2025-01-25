namespace Interpreter;

public class Program
{
    private static bool hadError = false;
    public static void Main(string[] args)
    {
        if (args.Length > 1)
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
        }
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

        var parser = new Parser(tokens);
        var expr = parser.Parse();

        if(hadError) return;

        Console.WriteLine(new AstPrinter().Print(expr));
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if(token.Type == TokenType.EOF)
        {
            Report(token.Line, "at end", message);
        }
        else
            {
                Report(token.Line, $"at '{token.Lexeme}'", message);
            }
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[{line}] Error {where}: {message}");
        hadError = true;
    }
}
