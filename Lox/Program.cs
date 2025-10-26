using Lox.Tokens;
using Lox.Visitors;

namespace Lox;

public class Program
{
    public static bool HadError { get; private set; }
    public static bool HadRuntimeError { get; private set; }

    private static readonly Interpreter Interpreter = new();

    public static void Main(string[] args)
    {
        ReadLine.AutoCompletionHandler = new AutoCompleteHandler();

        if (args.Length > 1)
        {
            Console.WriteLine("Usage: lox [script]");
            System.Environment.Exit(1);
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
            System.Environment.Exit(65);
        if (HadRuntimeError)
            System.Environment.Exit(70);
    }

    private static void RunPrompt()
    {
        while (true)
        {

#if DEBUG
            var line = Console.ReadLine();
#else
            var line = ReadLine.Read("> ");
#endif

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
        var statements = parser.Parse();

        if (HadError || statements is null) return;

        var resolver = new Resolver(Interpreter);
        resolver.Resolve(statements);

        if (HadError) return;

        Interpreter.Interpret(statements);
    }

    public static void Error(int line, string message)
    {
        Report(line, string.Empty, message);
    }

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
            Report(token.Line, $"at '{token.Lexeme}'", message);
        else
            Report(token.Line, $"at '{token.Lexeme}'", message);
    }

    public static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine($@"{error.Message}
[line {error.Token.Line}]");
        HadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error {where}: {message}");
        HadError = true;
    }
}