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


