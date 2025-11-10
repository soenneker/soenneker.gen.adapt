namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines.Base;

public record BaseOutboundRequest
{
    public string? AgentId { get; set; }
    public EndpointConfigRequest? Connector { get; set; }
}

