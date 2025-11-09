namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines;

public sealed class AdapterConnector
{
    public string? Name { get; set; }
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
}

public sealed class AdapterConnectorDocument
{
    public string? Name { get; set; }
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
}

public sealed class AdapterIntegrationDefinition
{
    public AdapterConnector? Primary { get; set; }
    public AdapterConnector? Secondary { get; set; }
    public Dictionary<string, AdapterConnector> Connectors { get; set; } = new();
}

public sealed class AdapterIntegrationDefinitionDocument
{
    public AdapterConnectorDocument? Primary { get; set; }
    public AdapterConnectorDocument? Secondary { get; set; }
    public Dictionary<string, AdapterConnectorDocument> Connectors { get; set; } = new();
}


