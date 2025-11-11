namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines;

public sealed class FlowSetup
{
    public FlowType Kind { get; set; } = FlowType.Unknown;
    public OutboundProfile? Outbound { get; set; }
    public MetadataPacket? Metadata { get; set; }
    public int? LagHours { get; set; }
}


