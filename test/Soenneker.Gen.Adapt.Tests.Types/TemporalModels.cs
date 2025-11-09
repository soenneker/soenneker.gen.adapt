using System;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Types;

public class TemporalSchedule
{
    public string Id { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public DateOnly? EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public List<DateOnly> Milestones { get; set; } = [];
    public List<Uri> Resources { get; set; } = [];
    public TemporalScheduleMetadata Metadata { get; set; } = new();
}

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

public class TemporalScheduleMetadata
{
    public Uri Overview { get; set; } = new("https://example.org/overview");
    public Uri? Support { get; set; }
    public DateOnly LastUpdated { get; set; }
    public TimeOnly LastUpdatedTime { get; set; }
}

public class TemporalScheduleMetadataViewModel
{
    public Uri Overview { get; set; } = new("https://example.org/overview");
    public Uri? Support { get; set; }
    public DateOnly LastUpdated { get; set; }
    public TimeOnly LastUpdatedTime { get; set; }
}

public class LinkProfile
{
    public string Name { get; set; } = string.Empty;
    public Uri Primary { get; set; } = new("https://example.org");
    public Uri? Secondary { get; set; }
    public List<Uri> Mirrors { get; set; } = [];
}

public class LinkProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public Uri Primary { get; set; } = new("https://example.org");
    public Uri? Secondary { get; set; }
    public IReadOnlyList<Uri> Mirrors { get; set; } = Array.Empty<Uri>();
}

