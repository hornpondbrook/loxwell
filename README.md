# Loxwell - A C# Implementation of Lox

Loxwell is a C# implementation of the **Lox** programming language, as described in Robert Nystrom's book [Crafting Interpreters](https://www.craftinginterpreters.com/the-lox-language.html). This project aims to provide an interpreter for Lox, written in C#, with features including lexical analysis, parsing, evaluation, and runtime execution.

## Features

Loxwell follows the **tree-walk interpreter** model and supports:

- **Lexical analysis (Scanning)** - Tokenizes Lox source code.
- **Parsing** - Implements a recursive descent parser.
- **Expression evaluation** - Evaluates expressions using an AST and visitor pattern.
- **Statements and control flow** - Supports print statements, variable declarations, and control flow structures like `if`, `while`, and `for`.
- **Functions and closures** - Implements user-defined functions with support for closures.
- **Classes and Inheritance** - Implements classes, instance properties, methods, `this`, and superclass method calls via `super`.
- **Static and dynamic scoping** - Resolves variable bindings and supports nested scopes.

## Installation

To run Loxwell, you need:
- **.NET SDK 6.0+** (or newer) installed on your system.
- A C# IDE like **Visual Studio** or **VS Code**.

Clone the repository:
```sh
git clone https://github.com/hornpondbrook/loxwell.git
cd loxwell
```

## Usage

Run the Lox interpreter from the command line:
```sh
dotnet run --project loxwell
```
You can then enter Lox code interactively or run Lox scripts:
```sh
dotnet run --project loxwell sample/fibonacci.lox
```

## Project Structure

```
loxwell/              # Lox interpreter source code
  ├── AstPrinter.cs   # AST pretty-printer
  ├── Expr.cs         # Expression AST nodes
  ├── Interpreter.cs  # Interpreter logic
  ├── Lox.cs          # Entry point
  ├── Parser.cs       # Recursive descent parser
  ├── Resolver.cs     # Static analysis for variable binding
  ├── Scanner.cs      # Lexical analysis (tokenizer)
  ├── Token.cs        # Token representation
  ├── Stmt.cs         # Statement AST nodes

sample/               # Example Lox programs
  ├── fibonacci.lox   # Fibonacci sequence script
  ├── class.lox       # Class and method examples

tool/                 # Code generation tools
```

## Development History

Loxwell was developed incrementally, with the following key milestones:

- **Dec 2024**: Implemented the scanner and tokenizer.
- **Dec 2024**: Added recursive descent parsing.
- **Jan 2025**: Introduced statement execution and variable scoping.
- **Jan 2025**: Implemented control flow (`if`, `while`, `for`).
- **Jan 2025**: Added function definitions and closures.
- **Jan 2025**: Implemented object-oriented programming features, including classes and inheritance.

## Contributing

Contributions are welcome! To contribute:
1. Fork the repository.
2. Create a new branch for your feature/fix.
3. Submit a pull request.

## License

This project is licensed under the **MIT License**.

## Acknowledgments

- **Robert Nystrom** for his book *Crafting Interpreters*, which inspired this implementation.
- The open-source C# community for development tools and libraries.

---

*Happy coding with Loxwell!*

