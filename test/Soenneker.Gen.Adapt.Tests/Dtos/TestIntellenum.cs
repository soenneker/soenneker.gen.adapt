namespace Soenneker.Gen.Adapt.Tests.Dtos;

public class TestIntellenum
{
    public int Value { get; private set; }

    private TestIntellenum(int value)
    {
        Value = value;
    }

    public static TestIntellenum From(int value)
    {
        return new TestIntellenum(value);
    }
}

