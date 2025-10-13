using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Primitives;

public class NullablePrimitivesSource
{
    public bool? BoolValue { get; set; }
    public int? IntValue { get; set; }
    public long? LongValue { get; set; }
    public double? DoubleValue { get; set; }
    public decimal? DecimalValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
    public Guid? GuidValue { get; set; }
}

