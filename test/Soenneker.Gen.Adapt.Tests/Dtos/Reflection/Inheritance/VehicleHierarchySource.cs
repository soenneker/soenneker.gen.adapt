namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public class VehicleSource
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
}

public class CarSource : VehicleSource
{
    public int NumberOfDoors { get; set; }
    public bool HasSunroof { get; set; }
}

public class SportCarSource : CarSource
{
    public int Horsepower { get; set; }
    public double TopSpeed { get; set; }
}

