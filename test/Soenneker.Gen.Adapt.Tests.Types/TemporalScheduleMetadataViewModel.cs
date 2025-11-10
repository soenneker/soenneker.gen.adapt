using System;

namespace Soenneker.Gen.Adapt.Tests.Types;

public class TemporalScheduleMetadataViewModel
{
    public Uri Overview { get; set; } = new("https://example.org/overview");
    public Uri? Support { get; set; }
    public DateOnly LastUpdated { get; set; }
    public TimeOnly LastUpdatedTime { get; set; }
}

