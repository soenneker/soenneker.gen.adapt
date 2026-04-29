using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Primitives;

public class MixedNullableDest
{
    public int NonNullableInt { get; set; }
    public int? NullableInt { get; set; }
    public int NullableToNonNullableInt { get; set; }
    public decimal NullableToNonNullableDecimal { get; set; }
    public int? NonNullableToNullableInt { get; set; }
    public decimal? NonNullableToNullableDecimal { get; set; }
    public string NonNullableString { get; set; }
    public DateTime NonNullableDateTime { get; set; }
    public DateTime? NullableDateTime { get; set; }
}

