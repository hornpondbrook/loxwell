namespace loxwell;

public abstract class Stmt {
  public interface Visitor<R> {
    R VisitBlockStmt (BlockStmt stmt);
    R VisitExpressionStmt (ExpressionStmt stmt);
    R VisitPrintStmt (PrintStmt stmt);
    R VisitVarStmt (VarStmt stmt);
  }
  public class BlockStmt : Stmt {
    public BlockStmt(List<Stmt> statements) {
      Statements = statements;
    }

    public override R Accept<R>(Visitor<R> visitor) {
      return visitor.VisitBlockStmt(this);
    }

    public readonly List<Stmt> Statements;
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
  public class VarStmt : Stmt {
    public VarStmt(Token name, Expr initializer) {
      Name = name;
      Initializer = initializer;
    }

    public override R Accept<R>(Visitor<R> visitor) {
      return visitor.VisitVarStmt(this);
    }

    public readonly Token Name;
    public readonly Expr Initializer;
  }

  public abstract R Accept<R>(Visitor<R>  visitor);
}
