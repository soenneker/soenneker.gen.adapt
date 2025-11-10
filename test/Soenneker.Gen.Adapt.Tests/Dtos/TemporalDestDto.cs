using System;
using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

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

