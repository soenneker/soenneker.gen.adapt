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


## **Simple**

| Method              |      Mean |        Ratio | Allocated | Alloc Ratio |
| ------------------- | --------: | -----------: | --------: | ----------: |
| Soenneker.Gen.Adapt |  4.954 ns |     baseline |      40 B |             |
| AutoMapper          | 34.880 ns | 7.14x slower |      40 B |  1.00x more |
| Mapster             | 12.639 ns | 2.59x slower |      40 B |  1.00x more |
| Mapperly            |  5.055 ns | 1.03x slower |      40 B |  1.00x more |
| Facet               |  6.068 ns | 1.24x slower |      40 B |  1.00x more |

---

## **Nested**

| Method              |      Mean |        Ratio | Allocated | Alloc Ratio |
| ------------------- | --------: | -----------: | --------: | ----------: |
| Soenneker.Gen.Adapt |  4.746 ns |     baseline |      32 B |             |
| AutoMapper          | 36.247 ns | 7.72x slower |      32 B |  1.00x more |
| Mapster             | 15.796 ns | 3.37x slower |      72 B |  2.25x more |
| Mapperly            |  5.064 ns | 1.08x slower |      32 B |  1.00x more |
| Facet               |  6.708 ns | 1.43x slower |      32 B |  1.00x more |

---

## **Complex List**

| Method              |     Mean |        Ratio | Allocated | Alloc Ratio |
| ------------------- | -------: | -----------: | --------: | ----------: |
| Soenneker.Gen.Adapt | 38.83 ns |     baseline |     320 B |             |
| AutoMapper          | 86.14 ns | 2.24x slower |     328 B |  1.02x more |
| Mapster             | 66.15 ns | 1.72x slower |     320 B |  1.00x more |
| Mapperly            | 41.54 ns | 1.08x slower |     320 B |  1.00x more |

---

## **Complex List (No nested conversions)**

| Method              |      Mean |         Ratio | Allocated | Alloc Ratio |
| ------------------- | --------: | ------------: | --------: | ----------: |
| Soenneker.Gen.Adapt |  3.735 ns |      baseline |      24 B |             |
| AutoMapper          | 47.883 ns | 12.93x slower |     112 B |  4.67x more |
| Mapster             | 49.255 ns | 13.30x slower |     320 B | 13.33x more |
| Mapperly            |  4.221 ns |  1.14x slower |      24 B |  1.00x more |
| Facet               |  4.521 ns |  1.22x slower |      24 B |  1.00x more |

---

## **Large Lists**

| Method              |      Mean |        Ratio | Allocated | Alloc Ratio |
| ------------------- | --------: | -----------: | --------: | ----------: |
| Soenneker.Gen.Adapt |  6.549 μs |     baseline |  46.93 KB |             |
| AutoMapper          | 10.655 μs | 1.64x slower |  55.27 KB |  1.18x more |
| Mapster             |  7.196 μs | 1.11x slower |  46.93 KB |  1.00x more |
| Mapperly            |  6.974 μs | 1.07x slower |  46.93 KB |  1.00x more |

---

## **Nested Lists**

| Method              |     Mean |        Ratio | Allocated | Alloc Ratio |
| ------------------- | -------: | -----------: | --------: | ----------: |
| Soenneker.Gen.Adapt | 1.033 μs |     baseline |   7.87 KB |             |
| AutoMapper          | 1.690 μs | 1.65x slower |   9.17 KB |  1.17x more |
| Mapster             | 1.175 μs | 1.14x slower |   7.87 KB |  1.00x more |
| Mapperly            | 1.143 μs | 1.11x slower |   7.87 KB |  1.00x more |

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
