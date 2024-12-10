using System.Text;

namespace loxwell;

/*
In reverse Polish notation (RPN), the operands to an arithmetic operator are both placed before the operator, so 1 + 2 becomes 1 2 +. Evaluation proceeds from left to right. Numbers are pushed onto an implicit stack. An arithmetic operator pops the top two numbers, performs the operation, and pushes the result. Thus, this:

(1 + 2) * (4 - 3)
in RPN becomes:

1 2 + 4 3 - *
Define a visitor class for our syntax tree classes that takes an expression, converts it to RPN, and returns the resulting string.
*/

// Generally need to implement a post order traversal of the AST tree

public class RpnPrinter : Expr.Visitor<string>
{

  static void Main(string[] args) {

    Expr expression = new Expr.Binary(
      new Expr.Binary(
        new Expr.Literal(1),
        new Token(TokenType.PLUS, "+", null, 1),
        new Expr.Literal(2)
      ),
      new Token(TokenType.STAR, "*", null, 1),
      new Expr.Binary(
        new Expr.Literal(4),
        new Token(TokenType.MINUS, "-", null, 1),
        new Expr.Literal(3)
      )
    );

    Console.WriteLine(new RpnPrinter().Print(expression));
  }

  string Print(Expr expr) {
    return expr.Accept(this);
  }

  string Output(string name, params Expr[] exprs) {
    StringBuilder sb = new StringBuilder();

    foreach (Expr expr in exprs) {
      sb.Append(expr.Accept(this)).Append(" ");
    }
    sb.Append(name);
    return sb.ToString();
  }

  public string VisitBinaryExpr(Expr.Binary expr)
  {
    return Output(expr.Operater.Lexeme, expr.Left, expr.Right);
  }

  public string VisitGroupingExpr(Expr.Grouping expr)
  {
    return Output("", expr.Expression);
  }

  public string VisitLiteralExpr(Expr.Literal expr)
  {
    return expr.Value?.ToString() ?? "nil";
  }

  public string VisitUnaryExpr(Expr.Unary expr)
  {
    throw new NotImplementedException();
  }
}