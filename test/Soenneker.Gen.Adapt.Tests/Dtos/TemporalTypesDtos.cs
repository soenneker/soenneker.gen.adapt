using System;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class TemporalSourceDto
{
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public DateOnly? OptionalDate { get; set; }
    public TimeOnly? OptionalTime { get; set; }
    public Uri ProfileLink { get; set; } = default!;
    public Uri? BackupLink { get; set; }
    public DateOnly[] ImportantDates { get; set; } = Array.Empty<DateOnly>();
    public List<TimeOnly> MeetingTimes { get; set; } = [];
    public Dictionary<string, DateOnly?> NamedDates { get; set; } = new();
    public IReadOnlyList<Uri> ResourceLinks { get; set; } = Array.Empty<Uri>();
    public HashSet<Uri> FavoriteLinks { get; set; } = [];
    public TemporalNestedSourceDto Nested { get; set; } = new();
}

public class TemporalDestDto
{
    public DateOnly Date { get; set; }
    public TimeOnly Time { get; set; }
    public DateOnly? OptionalDate { get; set; }
    public TimeOnly? OptionalTime { get; set; }
    public Uri ProfileLink { get; set; } = default!;
    public Uri? BackupLink { get; set; }
    public List<DateOnly> ImportantDates { get; set; } = [];
    public TimeOnly[] MeetingTimes { get; set; } = Array.Empty<TimeOnly>();
    public Dictionary<string, DateOnly?> NamedDates { get; set; } = new();
    public List<Uri> ResourceLinks { get; set; } = [];
    public HashSet<Uri> FavoriteLinks { get; set; } = [];
    public TemporalNestedDestDto Nested { get; set; } = new();
}

public class TemporalNestedSourceDto
{
    public DateOnly EffectiveDate { get; set; }
    public TimeOnly CutoffTime { get; set; }
    public Uri Documentation { get; set; } = default!;
    public Uri? SupportLink { get; set; }
}

public class TemporalNestedDestDto
{
    public DateOnly EffectiveDate { get; set; }
    public TimeOnly CutoffTime { get; set; }
    public Uri Documentation { get; set; } = default!;
    public Uri? SupportLink { get; set; }
}

