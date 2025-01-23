
namespace loxwell;

public class LoxClass : LoxCallable {
  public readonly string Name;
  public readonly Dictionary<string, LoxFunction> Methods;

  public LoxClass(string name, Dictionary<string, LoxFunction> methods) {
    Name = name;
    Methods = methods;
  }

  public int Arity() {
    LoxFunction initializer = FindMethod("this");
    if (initializer == null) return 0;
    return initializer.Arity();
  }

  public object Call(Interpreter interpreter, List<object> arguments) {
    LoxInstance instance = new LoxInstance(this);
    LoxFunction initializer = FindMethod("init");
    if (initializer != null) {
      initializer.Bind(instance).Call(interpreter, arguments);
    }
    return instance;
  }

  public LoxFunction FindMethod(string name) {
    if (Methods.ContainsKey(name)) {
      return Methods[name];
    }
    return null;
  }

  public override string ToString() {
    return Name;
  }
}