namespace loxwell;

public abstract class Stmt {
  public interface Visitor<R> {
    R VisitExpressionStmt (ExpressionStmt stmt);
    R VisitPrintStmt (PrintStmt stmt);
  }
  public class ExpressionStmt : Stmt {
    public ExpressionStmt(Expr expression) {
      Expression = expression;
    }

    public override R Accept<R>(Visitor<R> visitor) {
      return visitor.VisitExpressionStmt(this);
    }

    public readonly Expr Expression;
  }
  public class PrintStmt : Stmt {
    public PrintStmt(Expr expression) {
      Expression = expression;
    }

    public override R Accept<R>(Visitor<R> visitor) {
      return visitor.VisitPrintStmt(this);
    }

    public readonly Expr Expression;
  }

  public abstract R Accept<R>(Visitor<R>  visitor);
}
