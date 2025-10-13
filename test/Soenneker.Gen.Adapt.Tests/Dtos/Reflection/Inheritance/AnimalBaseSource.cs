namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public abstract class AnimalBaseSource
{
    public string Name { get; set; }
    public int Age { get; set; }
}

public class DogSource : AnimalBaseSource
{
    public string Breed { get; set; }
    public bool IsGoodBoy { get; set; }
}

public class CatSource : AnimalBaseSource
{
    public int LivesRemaining { get; set; }
    public bool LikesLasagna { get; set; }
}

