namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Complex;

public class DeepNestedSource
{
    public string Name { get; set; }
    public Level1Source Level1 { get; set; }
}

public class Level1Source
{
    public int Id { get; set; }
    public Level2Source Level2 { get; set; }
}

public class Level2Source
{
    public string Value { get; set; }
    public Level3Source Level3 { get; set; }
}

public class Level3Source
{
    public double Number { get; set; }
    public bool Flag { get; set; }
}

