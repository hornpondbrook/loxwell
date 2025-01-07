namespace loxwell;

using System.Collections;
using static TokenType;

public class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
{
  public void Interpret(List<Stmt> statements) {
    try {
      foreach (Stmt statement in statements) {
        Execute(statement);
      }
    } catch (RuntimeError error) {
      Lox.RuntimeError(error);
    }
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

  public object VisitExpressionStmt(Stmt.ExpressionStmt stmt)
  {
    Evaluate(stmt.Expression);
    return null;
  }

  public object VisitPrintStmt(Stmt.PrintStmt stmt)
  {
    object value = Evaluate(stmt.Expression);
    Console.WriteLine(Stringify(value));    
    return null;
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

  private string Stringify (object obj) {
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