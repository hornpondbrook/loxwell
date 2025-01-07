namespace loxwell;

using System.Collections.Concurrent;
using static TokenType;

// Parse for the following expression grammar
/*
expression     → equality ;
equality       → comparison ( ( "!=" | "==" ) comparison )* ;
comparison     → term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
term           → factor ( ( "-" | "+" ) factor )* ;
factor         → unary ( ( "/" | "*" ) unary )* ;
unary          → ( "!" | "-" ) unary
               | primary ;
primary        → NUMBER | STRING | "true" | "false" | "nil"
               | "(" expression ")" ;
*/

// Parse for the program with two types of statement
/*
program        → statement* EOF ;
statement      → exprStmt | printStmt ;
exprStmt       → expression ";" ;
printStmt      → "print" expression ";" ;
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
      statements.Add(Statement());
    }

    return statements;
  }

  private Stmt Statement() {
    if (Match(PRINT)) return PrintStatement();

    return ExpressionStatement();
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
    return Equality();
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

    return Primary();
  }

  private Expr Primary() {
    if (Match(TRUE)) return new Expr.Literal(true);
    if (Match(FALSE)) return new Expr.Literal(false);
    if (Match(NIL)) return new Expr.Literal(null);

    if (Match(NUMBER, STRING)) return new Expr.Literal(Previous().Literal);

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