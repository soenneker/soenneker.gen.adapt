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

## Generated mappings and runtime fallback

Known type pairs use direct construction, property access, and typed collection operations. Enum names and flags formatting are generated at compile time. Generic dispatch uses type identity comparisons; these do not inspect members or construct types dynamically.

When a call needs runtime type information, warning `SGA005` identifies the call site. Sources typed as `object`, open generic wrappers, and explicit `AdaptViaReflection<TDestination>()` calls remain supported. The fallback first resolves any available generated mapping for the runtime type pair, then uses reflection if necessary, caching that decision. The fallback is also available to late-generated Razor calls; its cache initializes only when the fallback is used, so ordinary generated mappings do not pay that initialization cost.

To keep a generic wrapper on the generated path, pass an explicit mapper:

```csharp
static TDestination Map<TSource, TDestination>(TSource source, Func<TSource, TDestination> mapper)
    => mapper(source);

User user = Map(dto, static value => value.Adapt<User>());
```

`Adapt<TDestination, TElement>()` uses typed constructors for collection shape changes with the same element type. Different element types use the warned fallback; prefer concrete `Adapt<TDestination>()` calls for generated element mappings. Runtime object mappings require constructible destination types and acyclic graphs; invalid constructors, throwing accessors, and invalid scalar values can still fail, as they can in handwritten mappings.

## Diagnostics and limitations

- `SGA002` reports a destination that cannot be constructed.
- `SGA003` reports a source/destination pair with no mappable properties.
- `SGA004` warns when a source type cannot be resolved.
- `SGA005` warns that a call can require runtime mapping and explains why. It is a warning, not a compilation error.
- Mapping is inferred from calls visible during compilation. Use concrete types where possible; generic and runtime-only calls retain fallback support.
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
