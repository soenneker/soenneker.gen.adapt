namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines;

public sealed class AdapterIntegrationDefinitionDocument
{
    public AdapterConnectorDocument? Primary { get; set; }
    public AdapterConnectorDocument? Secondary { get; set; }
    public Dictionary<string, AdapterConnectorDocument> Connectors { get; set; } = new();
}


