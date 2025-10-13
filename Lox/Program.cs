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

        foreach (var token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, string.Empty, message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line}] Error ${where}: {message}");
        HadError = true;
    }
}