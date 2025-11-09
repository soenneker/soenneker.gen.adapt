using Soenneker.Tests.Unit;
using Xunit;
using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Types.Pipelines;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class PipelineMappingTests : UnitTest
{
    public PipelineMappingTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_FlowSetupRequest_ToFlowSetup_ShouldGenerateNestedMappers()
    {
        var source = new FlowSetupRequest
        {
            Kind = FlowType.Outbound,
            LagHours = 3,
            Metadata = new MetadataPacket
            {
                Serialized = "payload"
            },
            Outbound = new OutboundRequest
            {
                AgentId = "agent-123",
                Connector = new EndpointConfigRequest
                {
                    Url = "https://example.com",
                    ApiKey = "key",
                    ApiToken = "token"
                }
            }
        };

        var result = source.Adapt<FlowSetup>();

        result.Kind.Should().Be(FlowType.Outbound);
        result.LagHours.Should().Be(3);
        result.Metadata.Should().NotBeNull();
        result.Metadata!.Serialized.Should().Be("payload");

        result.Outbound.Should().NotBeNull();
        result.Outbound!.AgentId.Should().Be("agent-123");
        result.Outbound.Connector.Should().NotBeNull();
        result.Outbound.Connector!.Url.Should().Be("https://example.com");
        result.Outbound.Connector.ApiKey.Should().Be("key");
        result.Outbound.Connector.ApiToken.Should().Be("token");
    }
}

