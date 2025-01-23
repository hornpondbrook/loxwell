
namespace loxwell;

public class LoxFunction : LoxCallable {
  private readonly Stmt.FunctionStmt _declaration;
  private readonly Context _closure;
  private readonly bool _isInitializer;

  public LoxFunction(Stmt.FunctionStmt declaration, Context closure,
                     bool isInitializer) {
    _declaration = declaration;
    _closure = closure;
    _isInitializer = isInitializer;
  }

  public LoxFunction Bind(LoxInstance instance) {
    Context context = new Context(_closure);
    context.Define("this", instance);
    return new LoxFunction(_declaration, context, _isInitializer);
  }

  public int Arity() {
    return _declaration.Parameters.Count;
  }

  public object Call(Interpreter interpreter, List<object> arguments) {
    Context context = new Context(_closure);
    for (int i = 0; i < _declaration.Parameters.Count; i++) {
      context.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
    }
    try {
      interpreter.ExecuteBlock(_declaration.Body, context);
    } catch (Return returnValue) {
      if (_isInitializer) return _closure.GetAt(0, "this");

      return returnValue.Value;
    }

    if (_isInitializer) return _closure.GetAt(0, "this");
    return null;
  }

  public override string ToString() {
    return $"<fn {_declaration.Name.Lexeme}>";
  }
}