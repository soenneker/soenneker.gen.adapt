using System.Collections.Generic;

namespace Soenneker.Gen.Adapt.Tests.Types.Banking;

public sealed class LedgerEntriesResponse
{
    public List<LedgerEntryResponse> Entries { get; set; } = new();
}

