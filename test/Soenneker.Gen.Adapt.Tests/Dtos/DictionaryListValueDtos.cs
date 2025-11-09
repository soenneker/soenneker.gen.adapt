using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Dtos;

public sealed class ShipmentDocument
{
    public string Code { get; set; } = string.Empty;
}

public sealed class PackageDocument
{
    public string Tracking { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}

public sealed class PackageEntity
{
    public string Tracking { get; set; } = string.Empty;
    public decimal Weight { get; set; }
}

public sealed class ShipmentLedgerSource
{
    public Dictionary<ShipmentDocument, List<PackageDocument>> Packages { get; set; } = new();
}

public sealed class ShipmentLedgerDest
{
    public Dictionary<ShipmentDocument, List<PackageEntity>> Packages { get; set; } = new();
}


