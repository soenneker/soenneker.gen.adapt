using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class NestedObjectDest
{
    public string Name { get; set; }
    public InnerObjectDest Inner { get; set; }
}

public class InnerObjectDest
{
    public int Id { get; set; }
    public string Value { get; set; }
    public DateTime Timestamp { get; set; }
}

