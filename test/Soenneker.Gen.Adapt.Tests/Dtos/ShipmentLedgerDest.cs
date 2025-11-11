using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public sealed class ShipmentLedgerDest
{
    public Dictionary<ShipmentDocument, List<PackageEntity>> Packages { get; set; } = new();
}


