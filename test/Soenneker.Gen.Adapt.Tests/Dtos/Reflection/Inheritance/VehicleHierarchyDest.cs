namespace Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;

public class VehicleDest
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
}

public class CarDest : VehicleDest
{
    public int NumberOfDoors { get; set; }
    public bool HasSunroof { get; set; }
}

public class SportCarDest : CarDest
{
    public int Horsepower { get; set; }
    public double TopSpeed { get; set; }
}

