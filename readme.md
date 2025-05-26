# AutoCollection

| Actions                                                                                                                                                                   | NuGet                                                                                                         |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------|
| [![main](https://github.com/ChaseFlorell/AutoCollection/actions/workflows/main.yml/badge.svg)](https://github.com/ChaseFlorell/AutoCollection/actions/workflows/main.yml)[![continuous](https://github.com/ChaseFlorell/AutoCollection/actions/workflows/continuous.yml/badge.svg)](https://github.com/ChaseFlorell/AutoCollection/actions/workflows/continuous.yml) [![inspect code](https://github.com/ChaseFlorell/AutoCollection/actions/workflows/inspect_code.yml/badge.svg)](https://github.com/ChaseFlorell/AutoCollection/actions/workflows/inspect_code.yml)  | [![NuGet](https://img.shields.io/nuget/v/AutoCollection.svg)](https://www.nuget.org/packages/AutoCollection/) |

A lightweight source generator library that automatically creates read-only collection wrappers for your types using the `GenerateReadOnlyList` attribute.

## Overview

AutoCollection eliminates boilerplate code by generating `IReadOnlyList<T>` implementations for your collection types. This allows you to create type-safe, immutable collection wrappers without writing repetitive code.

## Installation

Install the AutoCollection package via NuGet:

```bash~~~~
dotnet add package AutoCollection
```

## Usage

AutoCollection provides a simple attribute-based API to generate read-only list implementations. There are two main ways to use this library:

### 1. Basic Usage with Default Implementation

Simply apply the attribute to a partial class and the source generator will create a complete implementation:

```csharp
using AutoCollection;

namespace Example;

// Apply the attribute to a partial class
[GenerateReadOnlyList(typeof(string))]
public partial class SimpleStringList { }

// Usage:
var stringList = new SimpleStringList(["one", "two", "three"]);
Console.WriteLine(stringList.Count);    // Outputs: 3
Console.WriteLine(stringList[0]);       // Outputs: "one"
```

### 2. Custom Implementation with Named Backing Field

You can specify a custom backing field name and provide your own constructor:

```csharp
using AutoCollection;
using System.Collections.Generic;
using System.Linq;

namespace Example;

[GenerateReadOnlyList(typeof(int), nameof(_values))]
public partial class CustomIntList
{
    public CustomIntList(IEnumerable<int> vals)
    {
        _values = vals?.ToArray() ?? throw new ArgumentNullException(nameof(vals));
    }

    private readonly int[] _values;
}

// Usage:
var intList = new CustomIntList([1, 2, 3]);
Console.WriteLine(intList.Count);    // Outputs: 3
Console.WriteLine(intList[1]);       // Outputs: 2
```

### 3. Working with Complex Types

The generator works with any type, including your own custom classes:

```csharp
using AutoCollection;

namespace Example;

public class Person
{
    public string FirstName { get; set; } = "John";
    public string LastName { get; set; } = "Doe";
    public string FullName => $"{FirstName} {LastName}";
}

[GenerateReadOnlyList(typeof(Person))]
public partial class PersonList { }

// Usage:
var people = new PersonList([new Person(), new Person()]);
Console.WriteLine(people.Count);              // Outputs: 2
Console.WriteLine(people[0].FullName);        // Outputs: "John Doe"
```

### 4. Basic Usage for IList<T>
```csharp
using AutoCollection;

namespace Example;

// Apply the attribute to a partial class
[GenerateList(typeof(string))]
public partial class SimpleStringList { }

// Usage:
var stringList = new SimpleStringList(["one", "two", "three"]);
Console.WriteLine(stringList.Count);    // Outputs: 3
Console.WriteLine(stringList[0]);       // Outputs: "one"
stringList.Add("Four");
Console.WriteLine(stringList[3]);       // Outputs: "four"
```

## Features

- **Zero runtime dependencies** - AutoCollection runs at build time through the C# source generator feature
- **Type safety** - Get compile-time checking for your collections
- **Customizable** - Use default implementation or provide your own backing field and constructor
- **Lightweight** - Generates minimal code with no performance overhead

## Generated Code

For a class like:

```csharp
[GenerateReadOnlyList(typeof(string))]
public partial class SimpleStringList { }
```

The source generator creates:

```csharp
public partial class SimpleStringList : IReadOnlyList<string>
{
    public SimpleStringList(IEnumerable<string> items)
    {
        _items = items?.ToArray() ?? throw new ArgumentNullException(nameof(items));
    }

    private readonly string[] _items;

    public string this[int index] => _items[index];
    public int Count => _items.Length;

    public IEnumerator<string> GetEnumerator() => ((IEnumerable<string>)_items).GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
```

## Requirements

- .NET 6.0 or higher
- C# 10.0 or higher

## License

AutoCollection is licensed under the MIT License.

## Contributing

Contributions are welcome! Feel free to submit issues or pull requests on the [GitHub repository](https://github.com/ChaseFlorell/AutoCollection).