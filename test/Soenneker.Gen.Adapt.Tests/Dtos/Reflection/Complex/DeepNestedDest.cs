namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class DeepNestedDest
{
    public string Name { get; set; }
    public Level1Dest Level1 { get; set; }
}

public class Level1Dest
{
    public int Id { get; set; }
    public Level2Dest Level2 { get; set; }
}

public class Level2Dest
{
    public string Value { get; set; }
    public Level3Dest Level3 { get; set; }
}

public class Level3Dest
{
    public double Number { get; set; }
    public bool Flag { get; set; }
}

