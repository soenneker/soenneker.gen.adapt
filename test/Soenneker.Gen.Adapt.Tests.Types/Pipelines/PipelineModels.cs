namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines;

public enum FlowType
{
    Unknown = 0,
    Outbound = 1
}

public sealed class MetadataPacket
{
    public string? Serialized { get; set; }
}

public sealed class EndpointConfigRequest
{
    public string? Url { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiToken { get; set; }
}

public sealed class EndpointConfig
{
    public string? Url { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiToken { get; set; }
}

public record BaseOutboundRequest
{
    public string? AgentId { get; set; }
    public EndpointConfigRequest? Connector { get; set; }
}

public abstract class BaseOutbound
{
    public string? AgentId { get; set; }
    public EndpointConfig? Connector { get; set; }
}

public sealed record OutboundRequest : BaseOutboundRequest;

public sealed class OutboundProfile : BaseOutbound
{
}

public sealed record FlowSetupRequest
{
    public FlowType Kind { get; set; } = FlowType.Unknown;
    public OutboundRequest? Outbound { get; set; }
    public MetadataPacket? Metadata { get; set; }
    public int? LagHours { get; set; }
}

public sealed class FlowSetup
{
    public FlowType Kind { get; set; } = FlowType.Unknown;
    public OutboundProfile? Outbound { get; set; }
    public MetadataPacket? Metadata { get; set; }
    public int? LagHours { get; set; }
}