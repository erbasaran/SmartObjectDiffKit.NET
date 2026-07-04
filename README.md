# SmartObjectDiffKit

[![NuGet Version](https://img.shields.io/nuget/v/SmartObjectDiffKit.svg)](https://www.nuget.org/packages/SmartObjectDiffKit)
[![License](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

**SmartObjectDiffKit** is a lightweight, high-performance, and enterprise-grade object comparison library for .NET. It allows you to compare any two object graphs (e.g. models, POCOs, collections, Dictionaries) and produce detailed, highly-configurable, human-readable, and machine-readable difference reports.

Optimized for low allocation and high throughput, it is ideal for audit logging, version history tracking, change detection, and automated integration testing.

---

## Key Features

- 🔍 **Deep Comparison**: Traversers deep nested object graphs, collections, and dictionaries.
- 🏷️ **Custom Display Names**: Customize property paths using `[DiffDisplayName("Custom Name")]`.
- 🔑 **Key-Based Collections**: Align elements in collections automatically by key property using `[DiffKey]`.
- ⚡ **High Performance**: Compiles and caches reflection getters and type metadata to minimize runtime overhead.
- 🔀 **Order-Insensitive Matching**: Match collection elements structurally or by key, disregarding their index position.
- 📝 **Multiple Exporters**: Generate output in **JSON**, **XML**, **Markdown**, **HTML**, **CSV**, **Plain Text**, and **Console Colorized** formats.
- 🛡️ **Zero Dependencies**: Core library targets **.NET Standard 2.0** with no external dependencies.
- 🔄 **Circular Reference Detection**: Built-in protection against infinite recursion.

---

## Installation

Install via NuGet Package Manager CLI:

```bash
dotnet add package SmartObjectDiffKit
```

---

## Quick Start

Compare two simple objects using the default configuration:

```csharp
using SmartObjectDiffKit;

var oldUser = new { Name = "John Doe", Age = 30 };
var newUser = new { Name = "Jane Doe", Age = 31 };

// Compare objects and get result
DiffResult result = ObjectDiffer.Create().Compare(oldUser, newUser);

Console.WriteLine(result.IsEqual);          // False
Console.WriteLine(result.DifferenceCount);   // 2

foreach (var diff in result.Differences)
{
    Console.WriteLine($"[{diff.DifferenceType}] {diff.PropertyPath}: '{diff.OldValue}' -> '{diff.NewValue}'");
}
// Output:
// [Modified] Name: 'John Doe' -> 'Jane Doe'
// [Modified] Age: '30' -> '31'
```

---

## Advanced Configurations

### 1. Key-Based Collection Comparison (`[DiffKey]`)

By default, lists are compared by index (ordered comparison). If you add, remove, or shuffle elements, this can lead to many incorrect modifications being reported. 

By decorating an identity property with `[DiffKey]`, the comparison engine automatically matches corresponding elements between the collections by their key:

```csharp
public class OrderItem
{
    [DiffKey]
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

var oldItems = new List<OrderItem>
{
    new() { ProductId = "PROD-A", Quantity = 5 },
    new() { ProductId = "PROD-B", Quantity = 2 }
};

var newItems = new List<OrderItem>
{
    // The items are swapped in position, and PROD-A quantity changed
    new() { ProductId = "PROD-B", Quantity = 2 },
    new() { ProductId = "PROD-A", Quantity = 8 }
};

var result = ObjectDiffer.Create().Compare(oldItems, newItems);

// It correctly matches PROD-A and reports the modified quantity, instead of mismatching elements!
// PropertyPath will reflect the key: ["PROD-A"].Quantity
var diff = result.Differences[0];
Console.WriteLine($"{diff.PropertyPath}: {diff.OldValue} -> {diff.NewValue}"); 
// Output: ["PROD-A"].Quantity: 5 -> 8
```

### 2. Custom Property Names (`[DiffDisplayName]`)

Use `[DiffDisplayName]` to change how properties are displayed in output reports (great for generating customer-facing audit logs):

```csharp
public class Employee
{
    [DiffDisplayName("Job Title")]
    public string Role { get; set; } = string.Empty;

    [DiffDisplayName("Monthly Salary")]
    public decimal Salary { get; set; }
}

var result = ObjectDiffer.Create().Compare(emp1, emp2);
// If Salary changes, the PropertyPath in the report will be "Monthly Salary" instead of "Salary".
```

### 3. Display Sorting (`[DiffOrder]`)

Control the order in which properties are evaluated and displayed in output files:

```csharp
public class Product
{
    [DiffOrder(1)]
    public string Name { get; set; } = string.Empty;

    [DiffOrder(2)]
    public decimal Price { get; set; }
}
```

### 4. Ignoring Properties

You can ignore properties in multiple ways:

```csharp
// 1. By decorating the property in code
public class Account
{
    [DiffIgnore]
    public string InternalToken { get; set; } = string.Empty;
}

// 2. By property name in configuration builder
var differ = ObjectDiffer.Create()
    .IgnoreProperty("LastModifiedDate")
    .Build();

// 3. By strong-typed lambda expression
var differ = ObjectDiffer.Create()
    .IgnoreProperty<Account>(x => x.InternalToken)
    .Build();

// 4. By generic predicate (e.g. ignore all properties starting with "Temp")
var differ = ObjectDiffer.Create()
    .IgnoreProperty((name, type) => name.StartsWith("Temp"))
    .Build();
```

### 5. Ignoring Collection Order globally

If elements do not have a defined `[DiffKey]`, you can still check for equality without regarding order by enabling `IgnoreCollectionOrder()`:

```csharp
var oldList = new List<int> { 1, 2, 3 };
var newList = new List<int> { 3, 1, 2 };

var result = ObjectDiffer.Create()
    .IgnoreCollectionOrder()
    .Compare(oldList, newList);

Console.WriteLine(result.IsEqual); // True
```
*Note: If complex objects do not have a `[DiffKey]`, the engine will fall back to deep structural verification to match them.*

---

## Exporting Reports

SmartObjectDiffKit provides rich extensions to serialize your diff reports into various formats.

```csharp
var result = ObjectDiffer.Create().Compare(oldObj, newObj);

string json     = result.ToJson(indented: true); // System.Text.Json format
string xml      = result.ToXml();                // Standard XML format
string markdown = result.ToMarkdown();           // Beautiful Markdown table report
string html     = result.ToHtml();               // Styled responsive HTML report
string csv      = result.ToCsv();                // Comma-separated values
string text     = result.ToPlainText();          // Clean tabular raw text
string console  = result.ToConsole();            // ANSI-friendly colorized console lines
```

### Example Markdown Output

```markdown
# Diff Report

**Status:** Different
**Differences:** 1
**Elapsed Time:** 2.45ms
**Objects Compared:** 5

## Differences

| Property Path | Type | Old Value | New Value | Severity |
|---|---|---|---|---|
| `Address.City` | Modified | Springfield | Shelbyville | Medium |
```

---

## Thread Safety & Caching

The library is completely thread-safe and optimized for production environments:
- **Immutable Configuration**: `ObjectDiffer` instances are immutable once constructed. You can register them as Singletons in dependency injection containers.
- **Compiled Delegates**: Caches compiled lambda getters inside `ConcurrentDictionary` to achieve near-native execution speed when reading property values dynamically.
- **Isolated Contexts**: Each comparison call spawns a private state container (`ComparisonContext`) ensuring thread isolation.

---

## Target Frameworks

- **SmartObjectDiffKit** (Core Library): `.NET Standard 2.0` (Runs on .NET Core, .NET 5/6/7/8/9/10, and .NET Framework 4.6.1+).
- **Benchmarks, Samples, and Tests**: `.NET 10.0`.

---

## License

This project is licensed under the **MIT License**.