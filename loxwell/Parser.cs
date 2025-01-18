namespace loxwell;

using System.Collections.Concurrent;
using System.Reflection.Metadata.Ecma335;
using static TokenType;

// Parse for the following expression grammar
/*
expression     → assignment ;
assignment     → IDENTIFIER "=" assignment
               | logic_or ;
logic_or       → logic_and ( "or" logic_and )* ;
logic_and      → equality ( "and" equality )* ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           → factor ( ( "-" | "+" ) factor )* ;
factor         → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary | call ;
call           → primary ( "(" arguments? ")" )* ;
arguments      → expression ( "," expression )* ;
primary        → "true" | "false" | "nil"
               | NUMBER | STRING
               | "(" expression ")"
               | IDENTIFIER ;
*/

// Parse for the program with two types of statement
/*
program        → declaration* EOF ;
declaration    → funDecl
               | varDecl
               | statement ;
funDecl        → "fun" function ;
function       → IDENTIFIER "(" parameters? ")" block ;     
parameters     → IDENTIFIER ( "," IDENTIFIER )* ;          
varDecl        → "var" IDENTIFIER ( "=" expression )? ";" ;              
statement      → exprStmt
               | ifStmt
               | whileStmt
               | forStmt
               | printStmt
               | returnStmt
               | block ;            
exprStmt       → expression ";" ;
ifStmt         → "if" "(" expression ")" statement
               ( "else" statement )? ;
whileStmt      → "while" "(" expression ")" statement ;
forStmt        → "for" "(" ( varDecl | exprStmt | ";" )
                 expression? ";"
                 expression? ")" statement ;
printStmt      → "print" expression ";" ;
returnStmt     → "return" expression? ";" ;
block          → "{" declaration* "}" ;   
*/


public class Parser {
  private class ParseError : Exception {}
  private readonly List<Token> _tokens;
  private int _current = 0;

  public Parser(List<Token> tokens) {
    _tokens = tokens;
  }

  public List<Stmt> Parse() {
    List<Stmt> statements = new List<Stmt>();
    while (!IsAtEnd()) {
      statements.Add(Declaration());
    }

    return statements;
  }

  private Stmt Declaration() {
    try {
      if (Match(FUN)) return Function("function");
      if (Match(VAR)) return VarDeclaration();
      
      return Statement();
    } catch (ParseError error) {
      Synchronize();
      return null;
    }

  }

  private Stmt Function(string kind) {
    Token name = Consume(IDENTIFIER, $"Expect {kind} name.");
    Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");
    List<Token> parameters = new List<Token>();
    if (!Check(RIGHT_PAREN)) {
      do {
        if (parameters.Count >= 255) {
          Error(Peek(), "Can't have more than 255 parameters.");
        }

        parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
      } while (Match(COMMA));
    }
    Consume(RIGHT_PAREN, "Expect ')' after parameters.");
    Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");
    List<Stmt> body = Block();
    return new Stmt.FunctionStmt(name, parameters, body);
  }

  private Stmt VarDeclaration() {
    Token name = Consume(IDENTIFIER, "Expected variable name.");

    Expr initializer = null;
    if (Match(EQUAL)) {
      initializer = Expression();
    }

    Consume(SEMICOLON, "Expected ';' after variable declaration.");
    return new Stmt.VarStmt(name, initializer);
  }

  private Stmt Statement() {
    if (Match(IF)) return IfStatement();
    if (Match(WHILE)) return WhileStatement();
    if (Match(FOR)) return ForStatement();
    if (Match(PRINT)) return PrintStatement();
    if (Match(RETURN)) return ReturnStatement();
    if (Match(LEFT_BRACE)) return new Stmt.BlockStmt(Block());

    return ExpressionStatement();
  }

  private Stmt ReturnStatement() {
    Token keyword = Previous();
    Expr value = null;
    if (!Check(SEMICOLON)) {
      value = Expression();
    }
    Consume(SEMICOLON, "Expect ';' after return value.");
    return new Stmt.ReturnStmt(keyword, value);
  }

  private Stmt ForStatement() {
    Consume(LEFT_PAREN, "Expect '(' after 'for'.");

    Stmt initializer;
    if (Match(SEMICOLON)) {
      initializer = null;
    } else if (Match(VAR)) {
      initializer = VarDeclaration();
    } else {
      initializer = ExpressionStatement();
    }

    Expr condition = null;
    if (!Match(SEMICOLON)) {
      condition = Expression();
    }
    Consume(SEMICOLON, "Expect ';' after loop condition.");

    Expr increment = null;
    if (!Match(SEMICOLON)) {
      increment = Expression();
    }
    Consume(RIGHT_PAREN, "Expect ')' after for condition.");
    
    Stmt body = Statement();
    if (increment != null) {
      body = new Stmt.BlockStmt(new List<Stmt>() { body, new Stmt.ExpressionStmt(increment) });
    }
    if (condition == null) condition = new Expr.Literal(true);
    body = new Stmt.WhileStmt(condition, body);
    if (initializer != null) {
      body = new Stmt.BlockStmt(new List<Stmt>() { initializer, body });
    }

    return body;
  }

  private Stmt WhileStatement() {
    Consume(LEFT_PAREN, "Expect '(' after 'while'.");
    Expr condition = Expression();
    Consume(RIGHT_PAREN, "Expect ')' after while condition.");

    Stmt body = Statement();

    return new Stmt.WhileStmt(condition, body);
  }

  private Stmt IfStatement() {
    Consume(LEFT_PAREN, "Expect '(' after 'if'.");
    Expr condition = Expression();
    Consume(RIGHT_PAREN, "Expect ')' after if condition.");

    Stmt thenBranch = Statement();
    Stmt elseBranch = null;
    if (Match(ELSE)) {
      elseBranch = Statement();
    }

    return new Stmt.IfStmt(condition, thenBranch, elseBranch);
  }

  private List<Stmt> Block() {
    List<Stmt> statements = new List<Stmt>();

    while (!Check(RIGHT_BRACE) && !IsAtEnd()) {
      statements.Add(Declaration());
    }

    Consume(RIGHT_BRACE, "Expect '}' after block.");
    return statements;
  }

  private Stmt PrintStatement() {
    Expr value = Expression();
    Consume(SEMICOLON, "Expect ';' after value.");
    return new Stmt.PrintStmt(value);
  }

  private Stmt ExpressionStatement() {
    Expr expr = Expression();
    Consume(SEMICOLON, "Expect ';' after value.");
    return new Stmt.ExpressionStmt(expr);
  }

  private Expr Expression() {
    return Assignment();
  }

  private Expr Assignment() {
    Expr expr = Or();

    if (Match(EQUAL)) {
      Token equals = Previous();
      Expr value = Assignment();

      if (expr is Expr.Variable variableExpr) {
        Token name = variableExpr.Name;
        return new Expr.Assign(name, value);
      }

      Error(equals, "Invalid assignment target.");
    }

    return expr;
  }

  private Expr Or() {
    Expr expr = And();

    while (Match(OR)) {
      Token operater = Previous();
      Expr right = And();
      expr = new Expr.Logical(expr, operater, right);
    }

    return expr;

  }

  private Expr And() {
    Expr expr = Equality();

    while (Match(AND)) {
      Token operater = Previous();
      Expr right = Equality();
      expr = new Expr.Logical(expr, operater, right);
    }

    return expr;
  }

  private Expr Equality() {
    Expr expr = Comparison();

    while (Match(BANG_EQUAL, EQUAL_EQUAL)) {
      Token operater = Previous();
      Expr right = Comparison();
      expr = new Expr.Binary(expr, operater, right);
    }

    return expr;
  }

  private Expr Comparison() {
    Expr expr = Term();

    while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL)) {
      Token operater = Previous();
      Expr right = Term();
      expr = new Expr.Binary(expr, operater, right);
    }

    return expr;
  }

  private Expr Term() {
    Expr expr = Factor();

    while (Match(MINUS, PLUS)) {
      Token operater = Previous();
      Expr right = Factor();
      expr = new Expr.Binary(expr, operater, right);
    }

    return expr;
  }

  private Expr Factor() {
    Expr expr = Unary();

    while (Match(SLASH, STAR)) {
      Token operater = Previous();
      Expr right = Unary();
      expr = new Expr.Binary(expr, operater, right);
    }

    return expr;
  }

  private Expr Unary() {
    if (Match(BANG, MINUS)) {
      Token operater = Previous();
      Expr right = Unary();
      return new Expr.Unary(operater, right);
    } 

    return Call();
  }

  private Expr Call() {
    Expr expr = Primary();

    while(true) {
      if (Match(LEFT_PAREN)) {
        expr = FinishCall(expr);
      } else {
        break;
      }
    }

    return expr;
  }

  private Expr FinishCall(Expr expr) {
    List<Expr> arguments = new List<Expr>();

    if (!Check(RIGHT_PAREN)) {
      do {
        if (arguments.Count >= 255) {
          Error(Peek(), "Can't have more than 255 arguments.");
        }
        arguments.Add(Expression());
      } while (Match(COMMA));
    }

    Token paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

    return new Expr.Call(expr, paren, arguments);
  }

  private Expr Primary() {
    if (Match(TRUE)) return new Expr.Literal(true);
    if (Match(FALSE)) return new Expr.Literal(false);
    if (Match(NIL)) return new Expr.Literal(null);

    if (Match(NUMBER, STRING)) return new Expr.Literal(Previous().Literal);

    if (Match(IDENTIFIER)) return new Expr.Variable(Previous());

    if (Match(LEFT_PAREN)) {
      Expr expr = Expression();
      Consume(RIGHT_PAREN, "Expect ')' after expression.");
      return new Expr.Grouping(expr);
    }

    throw Error(Peek(), "Expect expression.");
  }
  
  private bool Match(params TokenType[] types) {
    foreach (TokenType type in types) {
      if (Check(type)) {
        Advance();
        return true;
      }
    }

    return false;
  }

  private Token Consume(TokenType type, string message) {
    if (Check(type)) return Advance();

    throw Error(Peek(), message);    
  }

  private ParseError Error(Token token, string message) {
    Lox.Error(token, message);
    return new ParseError();
  }

  private void Synchronize() {
    Advance();

    while (!IsAtEnd()) {
      if (Previous().Type == SEMICOLON) return;

      switch (Peek().Type) {
        case CLASS:
        case FUN:
        case VAR:
        case FOR:
        case IF:
        case WHILE:
        case PRINT:
        case RETURN:
          return;
      }

      Advance();
    }
  }

  private bool Check(TokenType type) {
    if (IsAtEnd()) return false;
    return Peek().Type == type;
  }

  private Token Advance() {
    if (!IsAtEnd()) _current++;
    return Previous();
  }

  private bool IsAtEnd() {
    return Peek().Type == EOF;
  }

  private Token Peek() {
    return _tokens[_current];
  }

  private Token Previous() {
    return _tokens[_current - 1];
  }
}