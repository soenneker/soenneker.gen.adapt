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


