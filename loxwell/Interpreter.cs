namespace loxwell;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata.Ecma335;
using static TokenType;

public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object> {

  public readonly Context Globals = new Context();
  private readonly Dictionary<Expr, int> _locals = new Dictionary<Expr, int>();
  private Context _context;

  private class Clock : LoxCallable {
    public int Arity() {
      return 0;
    }

    public object Call(Interpreter interpreter, List<object> arguments) {
      return (long)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
    }

    public override string ToString() {
      return "<native fn>";
    }
  }

  public Interpreter() {
    _context = Globals;

    Globals.Define("clock", new Clock());
  }
  public void Interpret(List<Stmt> statements) {
    try {
      foreach (Stmt statement in statements) {
        Execute(statement);
      }
    } catch (RuntimeError error) {
      Lox.RuntimeError(error);
    }
  }

  public object VisitLogicalExpr(Expr.Logical expr) {
    object left = Evaluate(expr.Left);

    if (expr.Operater.Type == OR) {
      if (IsTruthy(left)) return left;
    } else if (expr.Operater.Type == AND) {
      if (!IsTruthy(left)) return left;
    }

    return Evaluate(expr.Right);
  }

  public object VisitBinaryExpr(Expr.Binary expr) {
    object left = Evaluate(expr.Left);
    object right = Evaluate(expr.Right);

    switch (expr.Operater.Type) {
      case GREATER:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left > (double)right;
      case GREATER_EQUAL:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left >= (double)right;
      case BANG_EQUAL:
        return !IsEquaL(left, right);
      case EQUAL_EQUAL:
        return IsEquaL(left, right);
      case LESS:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left < (double)right;
      case LESS_EQUAL:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left <= (double)right;
      case MINUS:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left - (double)right;
      case SLASH:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left / (double)right;
      case STAR:
        CheckNumberOperand(expr.Operater, left, right);
        return (double)left * (double)right;
      case PLUS:
        if (left is double && right is double) {
          return (double)left + (double)right;
        }
        if (left is string && right is string) {
          return (string)left + (string)right;
        }
        throw new RuntimeError(expr.Operater,
          "Operands must be two numbers or two strings");
    }

    // Unreachable.
    return null;
  }

  public object VisitGroupingExpr(Expr.Grouping expr) {
    return Evaluate(expr.Expression);
  }

  public object VisitLiteralExpr(Expr.Literal expr) {
    return expr.Value;
  }

  public object VisitUnaryExpr(Expr.Unary expr) {
    object right = Evaluate(expr.Right);

    switch (expr.Operater.Type) {
      case BANG: return !IsTruthy(right);
      case MINUS:
        CheckNumberOperand(expr.Operater, right);
        return -(double)right;
    }

    return null;
  }

  public object VisitVariableExpr(Expr.Variable expr) {
    return LookupVariable(expr.Name, expr);
  }

  private object LookupVariable(Token name, Expr expr) {
    if (_locals.TryGetValue(expr, out int distance)) {
      return _context.GetAt(distance, name.Lexeme);
    } else {
      return Globals.Get(name);
    }
  }

  public object VisitAssignExpr(Expr.Assign expr) {
    object value = Evaluate(expr.Value);

    if (_locals.TryGetValue(expr, out int distance)) {
      _context.AssignAt(distance, expr.Name, value);
    } else {
      Globals.Assign(expr.Name, value);
    }

    return value;
  }

  public object VisitCallExpr(Expr.Call expr) {
    object callee = Evaluate(expr.Callee);

    List<object> arguments = new List<object>();
    foreach (Expr argument in expr.Arguments) {
      arguments.Add(Evaluate(argument));
    }

    if (callee is LoxCallable function) {
      if (arguments.Count != function.Arity()) {
        throw new RuntimeError(expr.Paren,
          $"Expected {function.Arity()} arguments but got {arguments.Count} .");
      }
      return function.Call(this, arguments);
    } else {
      throw new RuntimeError(expr.Paren,
        "Can only call functions and classes.");
    }
  }

  public object VisitGetExpr(Expr.Get expr) {
    object instance = Evaluate(expr.Instance);
    if (instance is LoxInstance loxInstance) {
      return loxInstance.Get(expr.Name);
    }

    throw new RuntimeError(expr.Name, "Only instances have properties.");
  }

  public object VisitSetExpr(Expr.Set expr) {
    object instance = Evaluate(expr.Instance);

    if (!(instance is LoxInstance)) {
      throw new RuntimeError(expr.Name, "Only instances have fields.");
    }

    object value = Evaluate(expr.Value);
    ((LoxInstance)instance).Set(expr.Name, value);
    return value;
  }

  public object VisitThisExpr(Expr.This expr) {
    return LookupVariable(expr.Keyword, expr);
  }

  public object VisitVarStmt(Stmt.VarStmt stmt) {
    object value = null;
    if (stmt.Initializer != null) {
      value = Evaluate(stmt.Initializer);
    }

    _context.Define(stmt.Name.Lexeme, value);
    return null;
  }

  public object VisitExpressionStmt(Stmt.ExpressionStmt stmt) {
    Evaluate(stmt.Expression);
    return null;
  }

  public object VisitPrintStmt(Stmt.PrintStmt stmt) {
    object value = Evaluate(stmt.Expression);
    Console.WriteLine(Stringify(value));
    return null;
  }

  public object VisitIfStmt(Stmt.IfStmt stmt) {
    if (IsTruthy(Evaluate(stmt.Condition))) {
      Execute(stmt.ThenBranch);
    } else if (stmt.ElseBranch != null) {
      Execute(stmt.ElseBranch);
    }
    return null;
  }

  public object VisitWhileStmt(Stmt.WhileStmt stmt) {
    while (IsTruthy(Evaluate(stmt.Condition))) {
      Execute(stmt.Body);
    }
    return null;
  }

  public object VisitBlockStmt(Stmt.BlockStmt stmt) {
    Context context = new Context(_context);
    ExecuteBlock(stmt.Statements, context);
    return null;
  }

  public object VisitFunctionStmt(Stmt.FunctionStmt stmt) {
    LoxFunction function = new LoxFunction(stmt, _context, false);
    _context.Define(stmt.Name.Lexeme, function);
    return null;
  }

  public object VisitReturnStmt(Stmt.ReturnStmt stmt) {
    object value = null;
    if (stmt.Value != null) value = Evaluate(stmt.Value);

    throw new Return(value);
  }

  public object VisitClassStmt(Stmt.ClassStmt stmt) {
    _context.Define(stmt.Name.Lexeme, null);

    Dictionary<string, LoxFunction> methods = new Dictionary<string, LoxFunction>();
    foreach (Stmt.FunctionStmt method in stmt.Methods) {
      LoxFunction function = new LoxFunction(method, _context,
        method.Name.Lexeme.Equals("init"));
      methods.Add(method.Name.Lexeme, function);
    }

    LoxClass klass = new LoxClass(stmt.Name.Lexeme, methods);
    _context.Assign(stmt.Name, klass);
    return null;
  }

  public void ExecuteBlock(List<Stmt> statements, Context context) {
    Context previous = _context;
    try {
      _context = context;

      foreach (Stmt stmt in statements) {
        Execute(stmt);
      }

    } finally {
      _context = previous;
    }
  }

  public void Resolve(Expr expr, int depth) {
    _locals[expr] = depth;
  }

  private void Execute(Stmt statement) {
    statement.Accept(this);
  }

  private object Evaluate(Expr expr) {
    return expr.Accept(this);
  }

  private bool IsTruthy(object obj) {
    if (obj == null) return false;
    if (obj is bool) return (bool)obj;
    return true;
  }

  private bool IsEquaL(object a, object b) {
    if (a == null && b == null) return true;
    if (a == null) return false;

    return a.Equals(b);
  }

  private void CheckNumberOperand(Token operater, params object[] operands) {
    foreach (object operand in operands) {
      if (!(operand is double))
        throw new RuntimeError(operater, "Operand must be a number");
    }

    return;
  }

  private string Stringify(object obj) {
    if (obj == null) return "nil";

    if (obj is double) {
      string text = obj.ToString()!;
      if (text.EndsWith(".0")) {
        text = text.Substring(0, text.Length - 2);
      }
      return text;
    }

    return obj.ToString()!;
  }

}
