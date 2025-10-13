namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public abstract class AnimalBaseDest
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class DogDest : AnimalBaseDest
{
    public string Breed { get; set; }
    public bool IsGoodBoy { get; set; }
}

public class CatDest : AnimalBaseDest
{
    public int LivesRemaining { get; set; }
    public bool LikesLasagna { get; set; }
}

