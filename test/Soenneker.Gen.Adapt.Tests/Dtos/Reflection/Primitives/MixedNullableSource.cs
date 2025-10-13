using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Primitives;

public class MixedNullableSource
{
    public int NonNullableInt { get; set; }
    public int? NullableInt { get; set; }
    public string NonNullableString { get; set; }
    public DateTime NonNullableDateTime { get; set; }
    public DateTime? NullableDateTime { get; set; }
}

