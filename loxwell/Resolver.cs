using System.Runtime.InteropServices;

namespace loxwell;

public class Resolver : Expr.Visitor<object>, Stmt.Visitor<object>
{

  private enum FunctionType
  {
    NONE,
    FUNCTION
  }

  private readonly Interpreter _interpreter;
  private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
  private FunctionType _currentFunction = FunctionType.NONE;

  public Resolver(Interpreter interpreter)
  {
    _interpreter = interpreter;
  }

  public void Resolve(List<Stmt> statements)
  {
    foreach (Stmt statement in statements)
    {
      Resolve(statement);
    }
  }

  public object VisitBlockStmt(Stmt.BlockStmt stmt)
  {
    BeginScope();
    Resolve(stmt.Statements);
    EndScope();
    return null;
  }

  public object VisitFunctionStmt(Stmt.FunctionStmt stmt)
  {
    Declare(stmt.Name);
    Define(stmt.Name);

    ResolveFunction(stmt, FunctionType.FUNCTION);
    return null;
  }

  public object VisitIfStmt(Stmt.IfStmt stmt)
  {
    Resolve(stmt.Condition);
    Resolve(stmt.ThenBranch);
    if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
    return null;
  }

  public object VisitPrintStmt(Stmt.PrintStmt stmt)
  {
    Resolve(stmt.Expression);
    return null;
  }

  public object VisitWhileStmt(Stmt.WhileStmt stmt)
  {
    Resolve(stmt.Condition);
    Resolve(stmt.Body);
    return null;
  }

  public object VisitReturnStmt(Stmt.ReturnStmt stmt)
  {
    if (_currentFunction == FunctionType.NONE)
    {
      Lox.Error(stmt.Keyword, "Can't return from top-level code.");
    }

    if (stmt.Value != null) Resolve(stmt.Value);
    return null;
  }

  public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
  {
    Resolve(stmt.Expression);
    return null;
  }

  public object VisitVarStmt(Stmt.VarStmt stmt)
  {
    Declare(stmt.Name);
    if (stmt.Initializer != null)
    {
      Resolve(stmt.Initializer);
    }
    Define(stmt.Name);
    return null;
  }

  private void BeginScope()
  {
    _scopes.Push(new Dictionary<string, bool>());
  }

  private void EndScope()
  {
    _scopes.Pop();
  }

  private void Declare(Token name)
  {
    if (_scopes.Count == 0) return;
    Dictionary<string, bool> scope = _scopes.Peek();
    if (scope.ContainsKey(name.Lexeme))
    {
      Lox.Error(name, "Already a variable with this name in this scope.");
    }
    scope[name.Lexeme] = false;
  }

  private void Define(Token name)
  {
    if (_scopes.Count == 0) return;
    Dictionary<string, bool> scope = _scopes.Peek();
    scope[name.Lexeme] = true;
  }

  private void ResolveFunction(Stmt.FunctionStmt function, FunctionType type)
  {
    FunctionType enclosingFunction = _currentFunction;
    _currentFunction = type;

    BeginScope();
    foreach (Token parameter in function.Parameters)
    {
      Declare(parameter);
      Define(parameter);
    }
    Resolve(function.Body);
    EndScope();

    _currentFunction = enclosingFunction;
  }

  private void Resolve(Stmt stmt)
  {
    stmt.Accept(this);
  }

  private void Resolve(Expr expr)
  {
    expr.Accept(this);
  }

  public object VisitAssignExpr(Expr.Assign expr)
  {
    Resolve(expr.Value);
    ResolveLocal(expr, expr.Name);
    return null;
  }

  public object VisitBinaryExpr(Expr.Binary expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Right);
    return null;
  }

  public object VisitCallExpr(Expr.Call expr)
  {
    Resolve(expr.Callee);
    foreach (Expr argument in expr.Arguments)
    {
      Resolve(argument);
    }
    return null;
  }

  public object VisitGroupingExpr(Expr.Grouping expr)
  {
    Resolve(expr.Expression);
    return null;
  }

  public object VisitLiteralExpr(Expr.Literal expr)
  {
    return null;
  }

  public object VisitLogicalExpr(Expr.Logical expr)
  {
    Resolve(expr.Left);
    Resolve(expr.Right);
    return null;
  }


  public object VisitUnaryExpr(Expr.Unary expr)
  {
    Resolve(expr.Right);
    return null;
  }

  public object VisitVariableExpr(Expr.Variable expr)
  {
    if (_scopes.Count != 0
        && _scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool value)
        && value == false)
    {
      Lox.Error(expr.Name,
        $"Can't read local variable {expr.Name.Lexeme} in its own initializer.");
    }
    ResolveLocal(expr, expr.Name);

    return null;
  }

  private void ResolveLocal(Expr expr, Token name)
  {
    int depth = 0;
    foreach (Dictionary<string, bool> scope in _scopes)
    {
      if (scope.ContainsKey(name.Lexeme))
      {
        _interpreter.Resolve(expr, depth);
        return;
      }
      depth++;
    }
  }


}