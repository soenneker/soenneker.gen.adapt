using Soenneker.Gen.EnumValues;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

/// <summary>
/// EnumValue type used to verify Soenneker.Gen.EnumValues and Soenneker.Gen.Adapt work together.
/// </summary>
[EnumValue]
public sealed partial class TestOrderStatus
{
    public static readonly TestOrderStatus Pending = new(1);
    public static readonly TestOrderStatus Completed = new(2);
}
