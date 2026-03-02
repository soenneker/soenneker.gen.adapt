namespace Soenneker.Gen.Adapt.Tests.Dtos;

/// <summary>
/// Simple class with public parameterless constructor - same-type mapping should produce a copy (new instance), not identity.
/// </summary>
public class CopyableItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
