namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines;

public sealed record FlowSetupRequest
{
    public FlowType Kind { get; set; } = FlowType.Unknown;
    public OutboundRequest? Outbound { get; set; }
    public MetadataPacket? Metadata { get; set; }
    public int? LagHours { get; set; }
}


