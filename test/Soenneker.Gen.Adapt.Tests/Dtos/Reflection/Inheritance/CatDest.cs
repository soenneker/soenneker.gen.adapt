using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance.Base;

namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public class CatDest : AnimalBaseDest
{
    public int LivesRemaining { get; set; }
    public bool LikesLasagna { get; set; }
}

