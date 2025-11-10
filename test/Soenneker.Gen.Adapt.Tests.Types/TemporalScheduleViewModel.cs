using System;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Types;

public class TemporalScheduleViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public IReadOnlyList<DateOnly> Milestones { get; set; } = Array.Empty<DateOnly>();
    public IReadOnlyList<Uri> Resources { get; set; } = Array.Empty<Uri>();
    public TemporalScheduleMetadataViewModel Metadata { get; set; } = new();
}

