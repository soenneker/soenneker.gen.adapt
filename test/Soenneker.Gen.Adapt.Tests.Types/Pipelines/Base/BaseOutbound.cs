namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines.Base;

public abstract class BaseOutbound
{
    public string? AgentId { get; set; }
    public EndpointConfig? Connector { get; set; }
}

