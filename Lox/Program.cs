using Lox.Generated;
using Lox.Tokens;
using Lox.Visitors;

namespace Lox;

public class Program
{
    public static bool HadError { get; private set; }

    public static void Main(string[] args)
    {
        ReadLine.AutoCompletionHandler = new AutoCompleteHandler();

        if (args.Length > 1)
        {
            Console.WriteLine("Usage: lox [script]");
            Environment.Exit(1);
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

    private static void RunFile(string path)
    {
        var content = File.ReadAllText(path);
        Run(content);

        if (HadError)
            Environment.Exit(1);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            var line = ReadLine.Read("> ");
            if (line is not null)
            {
                ReadLine.AddHistory(line);
                Run(line);
            }

            HadError = false;
        }
    }

    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        var parser = new Parser(tokens);
        var expr = parser.Parse();

        if (HadError || expr is null) return;

        Console.WriteLine(new ASTPrinter().Print(expr));
    }

    public static void Error(int line, string message)
    {
        Report(line, string.Empty, message);
    }

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
            Report(token.Line, $" at '{token.Lexeme}'", message);
        else
            Report(token.Line, $" at '{token.Lexeme}'", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error ${where}: {message}");
        HadError = true;
    }
}