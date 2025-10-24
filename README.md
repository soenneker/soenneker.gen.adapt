[![](https://img.shields.io/nuget/v/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.adapt/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.gen.adapt/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Gen.Adapt

A modern, high-performance C# source generator for compile-time object mapping; a zero-overhead replacement for AutoMapper, Mapperly, Mapster, etc.

- **Zero runtime cost** - All code is generated at compile time
- **Type-safe** - Compiler errors instead of runtime failures
- **Highly optimized** - Aggressive inlining, cached delegates, safe parsing
- **Works with Blazor** - Runs before Blazor souce generators.
- **Super easy to use** - Just call `Adapt<T>()` on your objects

## Installation

```bash
dotnet add package Soenneker.Gen.Adapt
```

## Usage

Once installed, the generator automatically detects usage of  `Adapt()` and generates extension methods for your types:

```csharp
public class UserDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public int Id { get; set; } // This property will be ignored since it doesn't exist in UserModel
}

public class UserModel
{
    public string Name { get; set; }
    public int Age { get; set; }
}

// Map objects with a simple extension method
var dto = new UserDto { Name = "John", Age = 30 };
UserModel model = dto.Adapt<UserModel>(); // just one line!
```

If the properties match by name and can be converted, it maps them. If for some reason the source generator cannot build the extension method, an Adapt() extension method will not be generated.

## What's being generated?

The generator creates extension methods with direct property assignments:

```csharp
public static partial class GenAdapt
{
    public static UserModel Adapt(this UserDto source)
    {
        var target = new UserModel();
        target.Name = source.Name;
        target.Age = source.Age;
        return target;
    }
}
```

### Supported Conversions

- **Same-type assignments** - Direct property copying
- **Collections** - `List<T>`, `IEnumerable<T>`, arrays with element conversion
- **Dictionaries** - `Dictionary<TKey, TValue>` with key/value conversion
- **Enums** - Bidirectional conversion between enum ↔ string, enum ↔ int
- **Nested objects** - Recursive mapping of complex object graphs
- **Nullables** - Automatic nullable handling

## Performance

### Compile-Time Mapping (`Adapt<T>()`)

**TL;DR**:
The generated code is generally faster than hand-written mapping code due to advanced object cloning.

* **No reflection**. Nor expression trees at runtime.
* **Direct assignments**. The generator emits straight property copies and optimized conversion.
* **Aggressive inlining and compiler optimization**. Hot paths are tiny and JIT-friendly.
* **Allocation aware**. Pre-sized collections, zero boxing, and no hidden intermediate lists.
* **Fast collection paths**. Direct span copying when possible; dictionary cloning uses efficient insertion patterns.
* **Reflection caching**. Code compilation is fast due to cached Reflection use during generation.


### Reflection Mapping (`AdaptViaReflection<TDest>()`)

For generic type parameters or abstract base classes where concrete types are only known at runtime:

- **Recursively adapts nested objects** - Automatically maps complex object graphs
- **Handles generic collections** - Adapts `List<SourceItem>` to `List<DestItem>` by recursively converting each item
- **Supports nested collections** - Works with `List<T>`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`
- Mappers are cached per type pair (first call overhead, then subsequent calls are fast)
- Use regular `Adapt<T>()` for better performance when types are known at compile time

## Benchmarking

**TL;DR**: Soenneker.Gen.Adapt is the fastest mapping library.


### **Simple mapping**

| Method                  |       Mean | Ratio         | Allocated | Alloc Ratio |
| ----------------------- | ---------: | ------------- | --------: | ----------: |
| **Soenneker.Gen.Adapt** |   4.143 ns | baseline      |      40 B |           — |
| AutoMapper              |  31.661 ns | 7.69x slower  |      40 B |  1.00x more |
| Mapster                 |   9.971 ns | 2.42x slower  |      40 B |  1.00x more |
| Mapperly                |   5.591 ns | 1.36x slower  |      40 B |  1.00x more |
| Facet                   | 183.466 ns | 44.55x slower |     296 B |  7.40x more |

---

### **Nested mapping**

| Method                  |       Mean | Ratio         | Allocated | Alloc Ratio |
| ----------------------- | ---------: | ------------- | --------: | ----------: |
| **Soenneker.Gen.Adapt** |   7.223 ns | baseline      |      72 B |           — |
| AutoMapper              |  34.976 ns | 4.88x slower  |      72 B |  1.00x more |
| Mapster                 |  14.085 ns | 1.97x slower  |      72 B |  1.00x more |
| Mapperly                |   8.590 ns | 1.20x slower  |      72 B |  1.00x more |
| Facet                   | 152.780 ns | 21.32x slower |     288 B |  4.00x more |

---

### **Nested list mapping**

| Method                  |       Mean | Ratio        | Allocated | Alloc Ratio |
| ----------------------- | ---------: | ------------ | --------: | ----------: |
| **Soenneker.Gen.Adapt** |   916.3 ns | baseline     |   7.87 KB |           — |
| AutoMapper              | 1,289.8 ns | 1.41x slower |   9.17 KB |  1.17x more |
| Mapster                 | 1,005.4 ns | 1.10x slower |   7.87 KB |  1.00x more |
| Mapperly                |   990.9 ns | 1.08x slower |   7.87 KB |  1.00x more |

---

### **Large list mapping**

| Method                  |     Mean | Ratio        | Allocated | Alloc Ratio |
| ----------------------- | -------: | ------------ | --------: | ----------: |
| **Soenneker.Gen.Adapt** | 5.473 μs | baseline     |  46.93 KB |           — |
| AutoMapper              | 7.885 μs | 1.45x slower |  55.27 KB |  1.18x more |
| Mapster                 | 5.708 μs | 1.05x slower |  46.93 KB |  1.00x more |
| Mapperly                | 5.647 μs | 1.04x slower |  46.93 KB |  1.00x more |

---

### **Complex list mapping**

| Method                  |      Mean | Ratio        | Allocated | Alloc Ratio |
| ----------------------- | --------: | ------------ | --------: | ----------: |
| **Soenneker.Gen.Adapt** |  40.56 ns | baseline     |     320 B |           — |
| AutoMapper              |  85.27 ns | 2.11x slower |     328 B |  1.02x more |
| Mapster                 |  62.97 ns | 1.56x slower |     320 B |  1.00x more |
| Mapperly                |  48.44 ns | 1.20x slower |     360 B |  1.12x more |
| Facet                   | 184.25 ns | 4.57x slower |     280 B |  1.14x less |

---

## Troubleshooting

The generator creates multiple files per source type (e.g., `Adapt.BasicSource.g.cs`, `Adapt.UserDto.g.cs`). To inspect the generated code, add this to your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will appear in `Project -> Dependencies -> Analyzers -> Soenneker.Gen.Adapt`

⚠️ Note: Source Generators don’t work transitively across project references.
Any project that calls `Adapt()` must include its own direct package reference.
