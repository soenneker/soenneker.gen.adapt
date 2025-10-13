using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class NestedObjectSource
{
    public string Name { get; set; }
    public InnerObjectSource Inner { get; set; }
}

public class InnerObjectSource
{
    public int Id { get; set; }
    public string Value { get; set; }
    public DateTime Timestamp { get; set; }
}

