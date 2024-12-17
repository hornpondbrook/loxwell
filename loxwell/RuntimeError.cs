namespace loxwell;

public class RuntimeError : Exception {
  internal readonly Token Token;

  public RuntimeError(Token token, string message) : base(message) {
    Token = token;
  }

}