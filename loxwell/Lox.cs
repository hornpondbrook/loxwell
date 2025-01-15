namespace loxwell;

public class Lox {
  private static readonly Interpreter interpreter = new Interpreter();
  private static bool _hasError = false;
  private static bool _hasRuntimeError = false;

  static void Main(string[] args) {
    if (args.Length > 1) {
      Console.WriteLine("Usage: loxwell [script]");
      Environment.Exit(64);
    } else if (args.Length == 1) {
      RunFile(args[0]);
    } else {
      RunPrompt();
    }

    Environment.Exit(0);
  }

  public static void Error(int line, string message) {
    Report(line, "", message);
  }

  public static void Error(Token token, string message) {
    if (token.Type == TokenType.EOF) {
      Report(token.Line, " at end", message);
    } else {
      Report(token.Line, $" at '{token.Lexeme}'", message);
    }
  }
  
  internal static void RuntimeError(RuntimeError error)
  {
    Console.WriteLine($"{error.Message}\n[line {error.Token.Line}]");
    _hasRuntimeError = true;
  }

  private static void RunFile(string path) {
    try {
      string fileContents = File.ReadAllText(path);
      // Console.WriteLine($"run file: {path}\n{fileContents}");
      Run(fileContents);
    } catch (IOException ex) {
      Console.WriteLine($"Error reading file: {ex.Message}");
    }

    // Indicate an error in the exit code
    if (_hasError) Environment.Exit(65);
    if (_hasRuntimeError) Environment.Exit(70);
  }

  private static void RunPrompt() {
    Console.WriteLine("Enter input (press Ctrl+D to exit):");
    for (;;) {
      Console.Write("> ");
      string line = Console.ReadLine();
      if (line == null) {
        Console.WriteLine("Existing...");
        break;
      }
      Run(line);
      _hasError = false;
    }
  }

  private static void Run(string source) {
    // Console.WriteLine($"run source:\n{source}");
    Scanner scanner = new Scanner(source);
    List<Token> tokens = scanner.ScanTokens();

    // For now, just print the tokens
    // foreach (Token token in tokens) {
    //   Console.WriteLine(token);
    // } 

    Parser parser = new Parser(tokens);
    List<Stmt> statements = parser.Parse();

    // Stop if there was a syntax error.
    if (_hasError) return;

    // Console.WriteLine(new AstPrinter().Print(expression));
    interpreter.Interpret(statements);
  }

  private static void Report(int line, string where, string message) {
    Console.WriteLine($"[line {line}] Error{where}: {message}");
    _hasError = true;
  }

}