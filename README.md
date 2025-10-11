[![](https://img.shields.io/nuget/v/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.adapt/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.gen.adapt/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Gen.Adapt

**A high-performance C# source generator for compile-time object mapping**

Generates strongly-typed mapping code at compile time with zero runtime overhead and no reflection.

## Why Use This?

- **Zero runtime cost** - All code is generated at compile time
- **No reflection** - Direct property assignments for maximum performance
- **Type-safe** - Compiler errors instead of runtime failures
- **IntelliSense support** - Full IDE support for generated methods

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
var model = dto.Adapt<UserModel>();
```

## What It Handles

Throw anything at it - classes, records, structs, nullables, nested objects, collections (`List<T>`, `IEnumerable<T>`, `Dictionary<K,V>`), enums (to/from string/int), Intellenums, whatever. If the properties match by name and can be converted, it maps them. 

If for some reason the source generator cannot build the extension method, you'll get a compile-time error.

## Performance

All mapping code is generated at compile time. No reflection, no runtime overhead. Mappers are cached as static delegates per source/destination type pair, so subsequent calls have virtually zero overhead beyond a direct property assignment.

### Generated Code Example

```csharp
public static class GenAdaptExtensions
{
    private static UserModel Map_UserDto_To_UserModel(UserDto source)
    {
        var target = new UserModel();
        target.Name = source.Name;
        target.Age = source.Age;
        return target;
    }

    private static class AdaptCache_UserDto<TDest>
    {
        public static readonly Func<UserDto, TDest> Invoke = BuildMapper();
        // Cached delegate - computed once, reused for the life of the application
    }

    public static TDest Adapt<TDest>(this UserDto source)
    {
        return AdaptCache_UserDto<TDest>.Invoke(source);
    }
}
```

### Troubleshooting

Behind the scenes, an Adapt.g.cs file is built, and can be inspected for debugging purposes by adding the following to your .csproj:

```xml
<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
<CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
```