using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public sealed class ShipmentLedgerSource
{
    public Dictionary<ShipmentDocument, List<PackageDocument>> Packages { get; set; } = new();
}


