using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class TemporalNestedSourceDto
{
    public DateOnly EffectiveDate { get; set; }
    public TimeOnly CutoffTime { get; set; }
    public Uri Documentation { get; set; } = default!;
    public Uri? SupportLink { get; set; }
}

