using System;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Base;

public class EntityBase
{
    public string Id { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

