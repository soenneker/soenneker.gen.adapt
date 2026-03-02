using Soenneker.Gen.EnumValues;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

/// <summary>
/// EnumValue&lt;string&gt; type used to verify Soenneker.Gen.EnumValues and Soenneker.Gen.Adapt work together.
/// </summary>
[EnumValue<string>]
public sealed partial class TestColorCode
{
    public static readonly TestColorCode Red = new("R");
    public static readonly TestColorCode Blue = new("B");
    public static readonly TestColorCode Green = new("G");
}
