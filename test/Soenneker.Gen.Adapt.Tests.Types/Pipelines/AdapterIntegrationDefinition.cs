using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Types.Pipelines;

public sealed class AdapterIntegrationDefinition
{
    public AdapterConnector? Primary { get; set; }
    public AdapterConnector? Secondary { get; set; }
    public Dictionary<string, AdapterConnector> Connectors { get; set; } = new();
}


