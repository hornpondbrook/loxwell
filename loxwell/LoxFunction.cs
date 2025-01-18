
namespace loxwell;

public class LoxFunction : LoxCallable
{
  private readonly Stmt.FunctionStmt _declaration;
  private readonly Context _closure;

  public LoxFunction(Stmt.FunctionStmt declaration, Context closure) {
    _declaration = declaration;
    _closure = closure;
  }
  public int Arity()
  {
    return _declaration.Parameters.Count;
  }

  public object Call(Interpreter interpreter, List<object> arguments)
  {
    Context context = new Context(_closure);
    for (int i = 0; i < _declaration.Parameters.Count; i++) {
      context.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
    }
    try {
      interpreter.ExecuteBlock(_declaration.Body, context);
    } catch (Return returnValue) {
      return returnValue.Value;
    }
    return null;
  }

  public override string ToString() {
    return $"<fn {_declaration.Name.Lexeme}>";
  }
}