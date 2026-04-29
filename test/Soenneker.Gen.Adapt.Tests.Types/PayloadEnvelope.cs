namespace Soenneker.Gen.Adapt.Tests.Types;

public sealed class PayloadEnvelope<T>
{
    public T? Payload { get; set; }
}
