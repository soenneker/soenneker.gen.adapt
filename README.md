[![](https://img.shields.io/nuget/v/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.adapt/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.gen.adapt/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Gen.Adapt

A modern, high-performance C# source generator for compile-time object mapping; a zero-overhead replacement for AutoMapper, Mapperly, Mapster, etc.

## Why Use This?

- **Zero runtime cost** - All code is generated at compile time
- **Type-safe** - Compiler errors instead of runtime failures
- **IntelliSense support** - Full IDE support for generated methods
- **Highly optimized** - Aggressive inlining, cached delegates, safe parsing

## Installation

```bash
dotnet add package Soenneker.Gen.Adapt
```

## Usage

Once installed, the generator automatically creates `Adapt<TDest>()` extension methods for your types:

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

If the properties match by name and can be converted, it maps them.

### Supported Conversions

- **Same-type assignments** - Direct property copying
- **Collections** - `List<T>`, `IEnumerable<T>`, arrays with element conversion
- **Dictionaries** - `Dictionary<TKey, TValue>` with key/value conversion
- **Enums** - Bidirectional conversion between enum ↔ string, enum ↔ int
- **Nested objects** - Recursive mapping of complex object graphs
- **Nullables** - Automatic nullable handling

If for some reason the source generator cannot build the extension method, an `Adapt()` extension method will not be generated.

## Performance

All mapping code is generated at compile time with multiple optimizations:

- **Zero reflection** - No expression trees, direct property assignments only
- **Aggressive inlining** - JIT compile methods when possible
- **Static delegate caching** - One-time initialization per source/destination pair
- **Minimal allocations** - Reused delegates, no boxing, no dynamic dispatch

The generated code is as fast as hand-written mapping code.

## What's being generated?

Take the example above. The generator creates efficient mapping code:

```csharp
using System.Runtime.CompilerServices;

public static partial class GenAdapt
{
    // Direct mapping method - no reflection, just property assignments
    private static UserModel Map_global__UserDto_To_global__UserModel(UserDto source)
    {
        var target = new UserModel();
        target.Name = source.Name;
        target.Age = source.Age;
        return target;
    }

    // Static delegate - computed once, zero allocations after initialization
    private static readonly Func<UserDto, UserModel> _map_UserDto_To_UserModel = 
        Map_global__UserDto_To_global__UserModel;

    private static class AdaptCache_global__UserDto<TDest>
    {
        public static readonly Func<UserDto, TDest> Invoke = BuildMapper();

        private static Func<UserDto, TDest> BuildMapper()
        {
            var destType = typeof(TDest);
            if (destType == typeof(UserModel))
                return (Func<UserDto, TDest>)(object)_map_UserDto_To_UserModel;
            throw new NotSupportedException($"Unsupported Adapt target type: {destType.FullName}");
        }
    }

    // Aggressive inlining for minimal call overhead
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TDest Adapt<TDest>(this UserDto source)
    {
        return AdaptCache_global__UserDto<TDest>.Invoke(source);
    }
}
```

## Troubleshooting

The generator creates multiple files per source type (e.g., `Adapt.BasicSource.g.cs`, `Adapt.UserDto.g.cs`). To inspect the generated code, add this to your `.csproj`:

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files will appear in `Project -> Dependencies -> Analyzers -> Soenneker.Gen.Adapt`
