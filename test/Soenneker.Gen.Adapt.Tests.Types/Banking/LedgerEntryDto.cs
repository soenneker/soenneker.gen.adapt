using System;

namespace Soenneker.Gen.Adapt.Tests.Types.Banking;

public sealed class LedgerEntryDto
{
    public string AccountId { get; set; } = string.Empty;
    public string? SellerId { get; set; }
    public string? SellerName { get; set; }
    public decimal Amount { get; set; }
    public decimal NetAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedUtc { get; set; }
}

