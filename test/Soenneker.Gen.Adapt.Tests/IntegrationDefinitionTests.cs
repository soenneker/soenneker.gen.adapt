using System;
using System.Collections.Generic;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Types.Banking;
using Soenneker.Gen.Adapt.Tests.Types.Pipelines;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class IntegrationDefinitionTests : UnitTest
{
    public IntegrationDefinitionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_IntegrationDefinition_WithNestedConnectors_ShouldMap_AllMembers()
    {
        var source = new AdapterIntegrationDefinition
        {
            Primary = new AdapterConnector
            {
                Name = "Primary",
                ApiKey = "p-key",
                Endpoint = "https://primary"
            },
            Secondary = new AdapterConnector
            {
                Name = "Secondary",
                ApiKey = "s-key",
                Endpoint = "https://secondary"
            },
            Connectors = new Dictionary<string, AdapterConnector>
            {
                ["alpha"] = new()
                {
                    Name = "Alpha",
                    ApiKey = "alpha-key",
                    Endpoint = "https://alpha"
                }
            }
        };

        var result = source.Adapt<AdapterIntegrationDefinitionDocument>();

        result.Primary.Should().NotBeNull();
        result.Primary!.Name.Should().Be("Primary");
        result.Primary.ApiKey.Should().Be("p-key");
        result.Secondary.Should().NotBeNull();
        result.Secondary!.Endpoint.Should().Be("https://secondary");
        result.Connectors.Should().ContainKey("alpha");
        result.Connectors!["alpha"].Endpoint.Should().Be("https://alpha");
    }

    [Fact]
    public void Adapt_List_FromRazorInferredTypes_ShouldMapEntries()
    {
        var response = new LedgerEntriesResponse
        {
            Entries =
            [
                new LedgerEntryResponse
                {
                    AccountId = "acct-1",
                    SellerId = "seller-1",
                    SellerName = "Seller",
                    Amount = 125.50m,
                    NetAmount = 120.25m,
                    Notes = "Initial deposit",
                    CreatedUtc = new DateTime(2025, 10, 1, 8, 30, 0, DateTimeKind.Utc)
                }
            ]
        };

        var result = response.Entries.Adapt<List<LedgerEntryDto>>();

        result.Should().HaveCount(1);
        result[0].AccountId.Should().Be("acct-1");
        result[0].SellerName.Should().Be("Seller");
        result[0].NetAmount.Should().Be(120.25m);
        result[0].CreatedUtc.Should().Be(new DateTime(2025, 10, 1, 8, 30, 0, DateTimeKind.Utc));
    }
}

