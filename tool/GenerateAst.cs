﻿using System.Net;
using System.Text;

namespace tool;

public class GenerateAst {

  static void Main(string[] args) {
    if (args.Length != 1) {
      Console.WriteLine("Usage: generate_ast <output directory>");
      Environment.Exit(64);
    }
    string outputDir = args[0];
    DefineAst(outputDir, "Expr", new List<string> {
      "Binary   : Expr left, Token operater, Expr right",
      "Grouping : Expr expression",
      "Literal  : Object value",
      "Unary    : Token operater, Expr right"      
    });

  }

  private static void DefineAst(string outputDir, string baseName, List<string> types)
  {
    string path = Path.Combine(outputDir, $"{baseName}.cs");

    try {
      using (StreamWriter writer = new StreamWriter(path)) {
        writer.WriteLine("namespace loxwell;");
        writer.WriteLine();
        writer.WriteLine($"public abstract class {baseName} {{");

        DefineVisitor(writer, baseName, types);

        // The AST classes.
        foreach (string type in types) {
          string className = type.Split(':')[0].Trim();
          string fields = type.Split(':')[1].Trim();
          DefineType(writer, baseName, className, fields);
        }

        // The base Accept() method.
        writer.WriteLine();
        writer.WriteLine("  public abstract R Accept<R>(Visitor<R>  visitor);");

        writer.WriteLine("}");
      }
    } catch (IOException ex) {
      Console.WriteLine($"Error writing to file: {ex.Message}");
    }
  }

  private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
  {
    writer.WriteLine($"  public interface Visitor<R> {{");

    foreach (string type in types) {
      string typeName = type.Split(':')[0].Trim();
      writer.WriteLine($"    R Visit{typeName}{baseName} ({typeName} {baseName.ToLower()});");
    }

    writer.WriteLine("  }");
  }

  private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
  {
    writer.WriteLine($"  public class {className} : {baseName} {{");

    // Constructor.
    writer.WriteLine($"    public {className}({fieldList}) {{");

    // Store parameters in fields.
    string[] fields = fieldList.Split(", ");
    foreach (string field in fields) {
      string name = field.Split(' ')[1];
      writer.WriteLine($"      {Capitalize(name)} = {name};");
    }

    writer.WriteLine("    }");

    // Visitor pattern.
    writer.WriteLine();
    writer.WriteLine($"    public override R Accept<R>(Visitor<R> visitor) {{");
    writer.WriteLine($"      return visitor.Visit{className}{baseName}(this);");
    writer.WriteLine("    }");

    // Fields
    writer.WriteLine("");
    foreach (string field in fields) {
      writer.WriteLine($"    public readonly {Capitalize(field)};");
    }

    writer.WriteLine("  }");
  }

  // Capitalize each word for the input string
  // Because in C# naming convention, public field is PascalCase and parameter is camelCase
  private static string Capitalize(string words) {
    if (string.IsNullOrEmpty(words)) return words;

    StringBuilder sb = new StringBuilder();
    string[] wordList = words.Split(' ');
    foreach (string word in wordList) {
      if (!string.IsNullOrWhiteSpace(word)) {
        sb.Append(char.ToUpper(word[0]));
        if (word.Length > 1) {
          sb.Append(word.Substring(1).ToLower());
        }
        sb.Append(" ");
      }
    }

    return sb.ToString().TrimEnd();
  }
}