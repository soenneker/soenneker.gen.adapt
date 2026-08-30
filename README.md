[![](https://img.shields.io/nuget/v/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.adapt/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.gen.adapt/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.gen.adapt.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.gen.adapt/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.gen.adapt/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.gen.adapt/actions/workflows/codeql.yml)

# Soenneker.Gen.Adapt

A C# source generator that discovers `Adapt<TDestination>()` calls and emits property-by-property object and collection mappings into the consuming project.

## Install

Add the package directly to every project that contains `Adapt` calls:

```bash
dotnet add package Soenneker.Gen.Adapt
```

Source generators do not flow transitively through ordinary project references, so installing it only in a shared library will not generate mappings in an application that references that library.

## Usage

The source needs readable properties and the destination needs writable properties with matching names. The destination normally needs an accessible parameterless constructor.

```csharp
public sealed class UserDto
{
    public string? Name { get; init; }
    public int Age { get; init; }
}

public sealed class User
{
    public string? Name { get; set; }
    public int Age { get; set; }
}

UserDto dto = new() { Name = "Ada", Age = 37 };
User user = dto.Adapt<User>();
```

The generator emits `GenAdapt` extension methods in a namespace derived from the consuming assembly name. If `Adapt` is not found, add a `using` for that namespace; hyphens in the assembly name are converted to underscores.

Only properties that have a compatible conversion are assigned. Destination-only properties retain the value supplied by the destination constructor or initializer.

## Supported mapping shapes

- Objects with matching property names, including nested object mappings discovered from the call graph.
- Arrays, common mutable and immutable collections, concurrent collections, and dictionaries.
- Nullable values and common scalar conversions.
- Enum, string, and integer conversions supported by the generated enum parsers.
- Same-type collection copies and same-type object copies when the type can be constructed.

Null source references return `null` from generated object mappings. Value-type sources are passed without that check.

## Runtime fallback

Use `AdaptViaReflection<TDestination>()` when the concrete source type is only known at runtime:

```csharp
object payload = GetPayload();
User user = payload.AdaptViaReflection<User>();
```

The reflection fallback caches a mapper for each runtime source/destination pair. It requires a destination that `Activator.CreateInstance` can construct, copies compatible public properties, and recursively handles supported nested objects and list-like collections. It is less capable and slower than generated mappings; incompatible properties are skipped. Use it with known destination types and acyclic object graphs.

## Diagnostics and limitations

- `SGA002` reports a destination that cannot be constructed.
- `SGA003` reports a source/destination pair with no mappable properties.
- `SGA004` warns when a source type cannot be resolved.
- Mapping is inferred from calls visible during compilation. A generic `Adapt<TDestination>()` invocation cannot be used as an unrestricted runtime mapper; requesting a destination for which no mapping was generated throws `NotSupportedException`.
- The generator does not provide configuration profiles, custom member expressions, or after-map hooks. Write explicit mapping code when names or business rules differ.
- Razor calls are discovered by scanning `.razor` additional files. Complex expressions that cannot be resolved may need to be assigned to a typed local before calling `Adapt`.

## Inspect generated code

```xml
<PropertyGroup>
  <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  <CompilerGeneratedFilesOutputPath>$(BaseIntermediateOutputPath)Generated</CompilerGeneratedFilesOutputPath>
</PropertyGroup>
```

Generated files also appear under the project's analyzer dependencies in IDEs that expose source-generator output.
