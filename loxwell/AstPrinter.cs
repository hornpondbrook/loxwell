using System.Text;

namespace loxwell;

public class AstPrinter : Expr.Visitor<string>
{

  // static void Main(string[] args) {
  //   Expr expression = new Expr.Binary(
  //       new Expr.Unary(
  //           new Token(TokenType.MINUS, "-", null, 1),
  //           new Expr.Literal(123)),
  //       new Token(TokenType.STAR, "*", null, 1),
  //       new Expr.Grouping(
  //           new Expr.Literal(45.67)));

  //   Console.WriteLine(new AstPrinter().Print(expression));    
  // }

  public string Print(Expr expr) {
    return expr.Accept(this);
  }

  public string VisitBinaryExpr(Expr.Binary expr)
  {
    return Parenthesize(expr.Operater.Lexeme, expr.Left, expr.Right);
  }

  public string VisitGroupingExpr(Expr.Grouping expr)
  {
    return Parenthesize("group", expr.Expression);
  }

  public string VisitLiteralExpr(Expr.Literal expr)
  {
    // if (expr.Value == null) return "nul";
    // return expr.Value.ToString();
    return expr.Value?.ToString() ?? "nul";
  }

  public string VisitUnaryExpr(Expr.Unary expr)
  {
    return Parenthesize(expr.Operater.Lexeme, expr.Right);
  }

  public string VisitVariableExpr(Expr.Variable expr)
  {
    throw new NotImplementedException();
  }

  public string VisitAssignExpr(Expr.Assign expr)
  {
    throw new NotImplementedException();
  }
  
  private string Parenthesize(string name, params Expr[] exprs) {
    StringBuilder sb = new StringBuilder();

    sb.Append("(").Append(name);
    foreach (Expr expr in exprs) {
      sb.Append(" ").Append(expr.Accept(this));
    }
    sb.Append(")");

    return sb.ToString();
  }

  public string VisitLogicalExpr(Expr.Logical expr)
  {
    throw new NotImplementedException();
  }

  public string VisitCallExpr(Expr.Call expr)
  {
    throw new NotImplementedException();
  }
}