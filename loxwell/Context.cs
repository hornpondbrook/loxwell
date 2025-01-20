namespace loxwell;

public class Context {

  public readonly Dictionary<string, object> Values = new Dictionary<string, object>();

  public readonly Context Enclosing;

  public Context() {
    Enclosing = null;
  }

  public Context(Context enclosing) {
    Enclosing = enclosing;
  }

  public void Define(string name, object value) {
    Values.Add(name, value);
  }

  public object Get(Token name) {
    if (Values.ContainsKey(name.Lexeme)) {
      return Values[name.Lexeme];
    }
    if (Enclosing != null) return Enclosing.Get(name);

    throw new RuntimeError(name, 
      $"Undefined variable '{name.Lexeme}'.");
  }

  public void Assign(Token name, object value) {
    if (Values.ContainsKey(name.Lexeme)) {
      Values[name.Lexeme] = value;
      return;
    }
    if (Enclosing != null) {
      Enclosing.Assign(name, value);
      return;
    } 

    throw new RuntimeError(name, 
      $"Undefined variable '{name.Lexeme}'.");
  }

  public object GetAt(int distance, string name) {
    return Ancestor(distance).Values[name];
  }

  public void AssignAt(int distance, Token name, object value) {
    Ancestor(distance).Values[name.Lexeme] = value;
  }

  private Context Ancestor(int distance) {
    Context context = this;
    for (int i = 0; i < distance; i++) {
      context = context.Enclosing;
    }
    return context;
  }

}