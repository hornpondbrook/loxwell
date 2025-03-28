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

## Architecture Overview

- **Project Structure**:
  - The project is divided into multiple files, each representing a specific component of the interpreter.
  - Key components include:
    - **Scanner**: Converts source code into tokens.
    - **Parser**: Converts tokens into an Abstract Syntax Tree (AST).
    - **Resolver**: Performs static analysis to resolve variable bindings and detect errors.
    - **Interpreter**: Executes the AST and evaluates expressions/statements.
    - **Runtime Components**: Includes classes for runtime errors, contexts, and callable objects (e.g., functions, classes).
    - **Tooling**: Includes a code generator (GenerateAst.cs) for generating AST node classes.
- **Workflow**:
  1. **Input**: The user provides Lox source code.
  2. **Scanning**: The Scanner tokenizes the source code into a list of Token objects.
  3. **Parsing**: The Parser converts the tokens into an AST using recursive descent parsing.
  4. **Static Analysis**: The Resolver traverses the AST to resolve variable bindings and detect semantic errors.
  5. **Interpretation**: The Interpreter walks the AST and executes the program.
  6. **Output**: Results are printed to the console or errors are reported.
- **Key Design Patterns**:
  - **Visitor Pattern**: Used for traversing and operating on the AST.
  - **Interpreter Pattern**: Implements the execution of the language.
  - **Factory Pattern**: Used in GenerateAst.cs to generate AST node classes.
- **Error Handling**:
  - **Compile-Time Errors**: Detected during scanning, parsing, or resolving.
  - **Runtime Errors**: Detected during interpretation and reported with context.
- **Extensibility**:
  - New language features can be added by updating the grammar, generating new AST nodes, and implementing corresponding visitor methods in the Interpreter and Resolver

```mermaid
graph TD
    A[Source Code] --> B[Scanner]
    B --> C[Tokens]
    C --> D[Parser]
    D --> E[Abstract Syntax Tree (AST)]
    E --> F[Resolver]
    F --> G[Interpreter]
    G --> H[Output / Runtime Errors]

    subgraph Runtime Components
        I[Context]
        J[LoxCallable]
        K[LoxClass]
        L[LoxInstance]
    end

    G --> I
    G --> J
    G --> K
    G --> L
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

