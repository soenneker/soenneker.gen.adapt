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

Simple mapping:

| Method           |       Mean |     Error |    StdDev |     Median |         Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
| ---------------- | ---------: | --------: | --------: | ---------: | ------------: | ------: | -----: | --------: | ----------: |
| Soenneker.Gen.Adapt         |   4.143 ns | 0.1179 ns | 0.3345 ns |   4.040 ns |      baseline |         | 0.0024 |      40 B |             |
| AutoMapper       |  31.661 ns | 0.6684 ns | 1.9498 ns |  31.205 ns |  7.69x slower |   0.76x | 0.0024 |      40 B |  1.00x more |
| Mapster |   9.971 ns | 0.2302 ns | 0.5733 ns |  10.127 ns |  2.42x slower |   0.23x | 0.0024 |      40 B |  1.00x more |
| Mapperly         |   5.591 ns | 0.1711 ns | 0.5044 ns |   5.494 ns |  1.36x slower |   0.16x | 0.0024 |      40 B |  1.00x more |
| Facet            | 183.466 ns | 3.6038 ns | 9.1729 ns | 180.937 ns | 44.55x slower |   4.09x | 0.0176 |     296 B |  7.40x more |

Nested mapping:

| Method           |       Mean |     Error |    StdDev |     Median |         Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
| ---------------- | ---------: | --------: | --------: | ---------: | ------------: | ------: | -----: | --------: | ----------: |
| Soenneker.Gen.Adapt         |   7.223 ns | 0.2264 ns | 0.6676 ns |   7.042 ns |      baseline |         | 0.0043 |      72 B |             |
| AutoMapper       |  34.976 ns | 0.8587 ns | 2.5318 ns |  34.269 ns |  4.88x slower |   0.55x | 0.0043 |      72 B |  1.00x more |
| Mapster |  14.085 ns | 0.3490 ns | 1.0181 ns |  14.312 ns |  1.97x slower |   0.22x | 0.0043 |      72 B |  1.00x more |
| Mapperly         |   8.590 ns | 0.2530 ns | 0.7460 ns |   8.318 ns |  1.20x slower |   0.15x | 0.0043 |      72 B |  1.00x more |
| Facet            | 152.780 ns | 3.0716 ns | 8.8623 ns | 149.429 ns | 21.32x slower |   2.24x | 0.0172 |     288 B |  4.00x more |

Nested list mapping:

| Method           |       Mean |    Error |   StdDev |     Median |        Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
| ---------------- | ---------: | -------: | -------: | ---------: | -----------: | ------: | -----: | -----: | --------: | ----------: |
| Soenneker.Gen.Adapt         |   916.3 ns | 18.36 ns | 49.64 ns |   907.7 ns |     baseline |         | 0.4807 | 0.0134 |   7.87 KB |             |
| AutoMapper       | 1,289.8 ns | 28.62 ns | 84.40 ns | 1,270.2 ns | 1.41x slower |   0.12x | 0.5608 | 0.0172 |   9.17 KB |  1.17x more |
| Mapster | 1,005.4 ns | 22.54 ns | 66.46 ns |   981.2 ns | 1.10x slower |   0.09x | 0.4807 | 0.0134 |   7.87 KB |  1.00x more |
| Mapperly         |   990.9 ns | 21.65 ns | 63.85 ns | 1,006.0 ns | 1.08x slower |   0.09x | 0.4807 | 0.0134 |   7.87 KB |  1.00x more |

Large list mapping:

| Method           |     Mean |     Error |    StdDev |   Median |        Ratio | RatioSD |   Gen0 |   Gen1 | Allocated | Alloc Ratio |
| ---------------- | -------: | --------: | --------: | -------: | -----------: | ------: | -----: | -----: | --------: | ----------: |
| Soenneker.Gen.Adapt         | 5.473 μs | 0.1355 μs | 0.3975 μs | 5.299 μs |     baseline |         | 2.8687 | 0.4425 |  46.93 KB |             |
| AutoMapper       | 7.885 μs | 0.1769 μs | 0.5187 μs | 7.974 μs | 1.45x slower |   0.14x | 3.3722 | 0.5493 |  55.27 KB |  1.18x more |
| Mapster | 5.708 μs | 0.1166 μs | 0.3437 μs | 5.606 μs | 1.05x slower |   0.10x | 2.8687 | 0.4425 |  46.93 KB |  1.00x more |
| Mapperly         | 5.647 μs | 0.1408 μs | 0.4152 μs | 5.684 μs | 1.04x slower |   0.10x | 2.8687 | 0.4425 |  46.93 KB |  1.00x more |

Complex list mapping:

| Method           |      Mean |    Error |    StdDev |    Median |        Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
| ---------------- | --------: | -------: | --------: | --------: | -----------: | ------: | -----: | --------: | ----------: |
| Soenneker.Gen.Adapt         |  40.56 ns | 1.023 ns |  3.016 ns |  40.15 ns |     baseline |         | 0.0191 |     320 B |             |
| AutoMapper       |  85.27 ns | 1.848 ns |  5.419 ns |  83.07 ns | 2.11x slower |   0.20x | 0.0196 |     328 B |  1.02x more |
| Mapster |  62.97 ns | 1.370 ns |  4.019 ns |  63.84 ns | 1.56x slower |   0.15x | 0.0191 |     320 B |  1.00x more |
| Mapperly         |  48.44 ns | 1.063 ns |  3.135 ns |  47.45 ns | 1.20x slower |   0.12x | 0.0215 |     360 B |  1.12x more |
| Facet            | 184.25 ns | 3.703 ns | 10.566 ns | 181.44 ns | 4.57x slower |   0.42x | 0.0167 |     280 B |  1.14x less |

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
