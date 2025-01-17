namespace loxwell;

public abstract class Stmt {
  public interface Visitor<R> {
    R VisitExpressionStmt (ExpressionStmt stmt);
    R VisitPrintStmt (PrintStmt stmt);
    R VisitIfStmt (IfStmt stmt);
    R VisitWhileStmt (WhileStmt stmt);
    R VisitBlockStmt (BlockStmt stmt);
    R VisitVarStmt (VarStmt stmt);
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
  public class IfStmt : Stmt {
    public IfStmt(Expr condition, Stmt thenBranch, Stmt elseBranch) {
      Condition = condition;
      ThenBranch = thenBranch;
      ElseBranch = elseBranch;
    }

    public override R Accept<R>(Visitor<R> visitor) {
      return visitor.VisitIfStmt(this);
    }

    public readonly Expr Condition;
    public readonly Stmt ThenBranch;
    public readonly Stmt ElseBranch;
  }
  public class WhileStmt : Stmt {
    public WhileStmt(Expr condition, Stmt body) {
      Condition = condition;
      Body = body;
    }

    public override R Accept<R>(Visitor<R> visitor) {
      return visitor.VisitWhileStmt(this);
    }

    public readonly Expr Condition;
    public readonly Stmt Body;
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
