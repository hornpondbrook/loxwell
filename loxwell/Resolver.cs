using System.Runtime.InteropServices;

namespace loxwell;

public class Resolver : Expr.Visitor<object>, Stmt.Visitor<object> {

  private enum FunctionType {
    NONE,
    FUNCTION,
    INITIALIZER,
    METHOD
  }

  private enum ClassType {
    NONE,
    CLASS,
    SUBCLASS
  }

  private readonly Interpreter _interpreter;
  private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
  private FunctionType _currentFunction = FunctionType.NONE;
  private ClassType _currentClass = ClassType.NONE;

  public Resolver(Interpreter interpreter) {
    _interpreter = interpreter;
  }

  public void Resolve(List<Stmt> statements) {
    foreach (Stmt statement in statements) {
      Resolve(statement);
    }
  }

  public object VisitBlockStmt(Stmt.BlockStmt stmt) {
    BeginScope();
    Resolve(stmt.Statements);
    EndScope();
    return null;
  }

  public object VisitFunctionStmt(Stmt.FunctionStmt stmt) {
    Declare(stmt.Name);
    Define(stmt.Name);

    ResolveFunction(stmt, FunctionType.FUNCTION);
    return null;
  }

  public object VisitIfStmt(Stmt.IfStmt stmt) {
    Resolve(stmt.Condition);
    Resolve(stmt.ThenBranch);
    if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
    return null;
  }

  public object VisitPrintStmt(Stmt.PrintStmt stmt) {
    Resolve(stmt.Expression);
    return null;
  }

  public object VisitWhileStmt(Stmt.WhileStmt stmt) {
    Resolve(stmt.Condition);
    Resolve(stmt.Body);
    return null;
  }

  public object VisitReturnStmt(Stmt.ReturnStmt stmt) {
    if (_currentFunction == FunctionType.NONE) {
      Lox.Error(stmt.Keyword, "Can't return from top-level code.");
    }

    if (stmt.Value != null) {
      if (_currentFunction == FunctionType.INITIALIZER) {
        Lox.Error(stmt.Keyword,
          "Can't return a value from an initializer.");
      }
      Resolve(stmt.Value);
    }

    return null;
  }

  public object VisitExpressionStmt(Stmt.ExpressionStmt stmt) {
    Resolve(stmt.Expression);
    return null;
  }

  public object VisitVarStmt(Stmt.VarStmt stmt) {
    Declare(stmt.Name);
    if (stmt.Initializer != null) {
      Resolve(stmt.Initializer);
    }
    Define(stmt.Name);
    return null;
  }

  public object VisitClassStmt(Stmt.ClassStmt stmt) {
    ClassType enclosingClass = _currentClass;
    _currentClass = ClassType.CLASS;

    Declare(stmt.Name);
    Define(stmt.Name);

    if (stmt.Superclass != null) {
      if (stmt.Name.Lexeme == stmt.Superclass.Name.Lexeme) {
        Lox.Error(stmt.Superclass.Name, "A class can't inherit from itself.");
      } else {
        _currentClass = ClassType.SUBCLASS;
        Resolve(stmt.Superclass);
      }
    }

    if (stmt.Superclass != null) {
      BeginScope();
      _scopes.Peek().Add("super", true);
    }

    BeginScope();
    _scopes.Peek().Add("this", true);

    foreach (Stmt.FunctionStmt method in stmt.Methods) {
      FunctionType declaration = FunctionType.METHOD;
      if (method.Name.Lexeme.Equals("init")) {
        declaration = FunctionType.INITIALIZER;
      }
      ResolveFunction(method, declaration);
    }

    EndScope();

    if (stmt.Superclass != null) EndScope();

    _currentClass = enclosingClass;
    return null;
  }

  private void BeginScope() {
    _scopes.Push(new Dictionary<string, bool>());
  }

  private void EndScope() {
    _scopes.Pop();
  }

  private void Declare(Token name) {
    if (_scopes.Count == 0) return;
    Dictionary<string, bool> scope = _scopes.Peek();
    if (scope.ContainsKey(name.Lexeme)) {
      Lox.Error(name, "Already a variable with this name in this scope.");
    }
    scope[name.Lexeme] = false;
  }

  private void Define(Token name) {
    if (_scopes.Count == 0) return;
    Dictionary<string, bool> scope = _scopes.Peek();
    scope[name.Lexeme] = true;
  }

  private void ResolveFunction(Stmt.FunctionStmt function, FunctionType type) {
    FunctionType enclosingFunction = _currentFunction;
    _currentFunction = type;

    BeginScope();
    foreach (Token parameter in function.Parameters) {
      Declare(parameter);
      Define(parameter);
    }
    Resolve(function.Body);
    EndScope();

    _currentFunction = enclosingFunction;
  }

  private void Resolve(Stmt stmt) {
    stmt.Accept(this);
  }

  private void Resolve(Expr expr) {
    expr.Accept(this);
  }

  public object VisitAssignExpr(Expr.Assign expr) {
    Resolve(expr.Value);
    ResolveLocal(expr, expr.Name);
    return null;
  }

  public object VisitBinaryExpr(Expr.Binary expr) {
    Resolve(expr.Left);
    Resolve(expr.Right);
    return null;
  }

  public object VisitCallExpr(Expr.Call expr) {
    Resolve(expr.Callee);
    foreach (Expr argument in expr.Arguments) {
      Resolve(argument);
    }
    return null;
  }

  public object VisitGroupingExpr(Expr.Grouping expr) {
    Resolve(expr.Expression);
    return null;
  }

  public object VisitLiteralExpr(Expr.Literal expr) {
    return null;
  }

  public object VisitLogicalExpr(Expr.Logical expr) {
    Resolve(expr.Left);
    Resolve(expr.Right);
    return null;
  }


  public object VisitUnaryExpr(Expr.Unary expr) {
    Resolve(expr.Right);
    return null;
  }

  public object VisitVariableExpr(Expr.Variable expr) {
    if (_scopes.Count != 0
        && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool value)
        && value == false) {
      Lox.Error(expr.Name,
        $"Can't read local variable {expr.Name.Lexeme} in its own initializer.");
    }
    ResolveLocal(expr, expr.Name);

    return null;
  }

  public object VisitGetExpr(Expr.Get expr) {
    Resolve(expr.Instance);
    return null;
  }

  public object VisitSetExpr(Expr.Set expr) {
    Resolve(expr.Value);
    Resolve(expr.Instance);
    return null;
  }

  public object VisitSuperExpr(Expr.Super expr) {
    if (_currentClass == ClassType.NONE) {
      Lox.Error(expr.Keyword,
        "Can't use 'this' outside of a class.");
    } else if (_currentClass != ClassType.SUBCLASS) {
      Lox.Error(expr.Keyword,
        "Can't use 'this' in a class with no superclass.");
    }

    ResolveLocal(expr, expr.Keyword);
    return null;
  }

  public object VisitThisExpr(Expr.This expr) {
    if (_currentClass == ClassType.NONE) {
      Lox.Error(expr.Keyword,
        "Can't use 'this' outside of a class.");
      return null;
    }

    ResolveLocal(expr, expr.Keyword);
    return null;
  }

  private void ResolveLocal(Expr expr, Token name) {
    int depth = 0;
    foreach (Dictionary<string, bool> scope in _scopes) {
      if (scope.ContainsKey(name.Lexeme)) {
        _interpreter.Resolve(expr, depth);
        return;
      }
      depth++;
    }
  }


}