using AwesomeAssertions;
using Soenneker.Gen.Adapt.Tests.Dtos.Reflection.Inheritance;
using Soenneker.Tests.Unit;
using Xunit;

namespace Soenneker.Gen.Adapt.Tests.Reflection;

public sealed class ReflectionInheritanceTests : UnitTest
{
    public ReflectionInheritanceTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void AdaptViaReflection_DogDerivedClass_ShouldMapAllProperties()
    {
        // Arrange
        var source = new DogSource
        {
            Name = "Buddy",
            Age = 5,
            Breed = "Golden Retriever",
            IsGoodBoy = true
        };

        // Act
        var result = source.AdaptViaReflection<DogDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Age.Should().Be(source.Age);
        result.Breed.Should().Be(source.Breed);
        result.IsGoodBoy.Should().Be(source.IsGoodBoy);
    }

    [Fact]
    public void AdaptViaReflection_CatDerivedClass_ShouldMapAllProperties()
    {
        // Arrange
        var source = new CatSource
        {
            Name = "Garfield",
            Age = 7,
            LivesRemaining = 8,
            LikesLasagna = true
        };

        // Act
        var result = source.AdaptViaReflection<CatDest>();

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(source.Name);
        result.Age.Should().Be(source.Age);
        result.LivesRemaining.Should().Be(source.LivesRemaining);
        result.LikesLasagna.Should().Be(source.LikesLasagna);
    }

    [Fact]
    public void AdaptViaReflection_VehicleBaseClass_ShouldMapBaseProperties()
    {
        // Arrange
        var source = new CarSource
        {
            Make = "Toyota",
            Model = "Camry",
            Year = 2024,
            NumberOfDoors = 4,
            HasSunroof = true
        };

        // Act
        var result = source.AdaptViaReflection<CarDest>();

        // Assert
        result.Should().NotBeNull();
        result.Make.Should().Be(source.Make);
        result.Model.Should().Be(source.Model);
        result.Year.Should().Be(source.Year);
        result.NumberOfDoors.Should().Be(source.NumberOfDoors);
        result.HasSunroof.Should().Be(source.HasSunroof);
    }

    [Fact]
    public void AdaptViaReflection_MultiLevelInheritance_ShouldMapAllLevels()
    {
        // Arrange
        var source = new SportCarSource
        {
            Make = "Ferrari",
            Model = "F8 Tributo",
            Year = 2024,
            NumberOfDoors = 2,
            HasSunroof = false,
            Horsepower = 710,
            TopSpeed = 211.3
        };

        // Act
        var result = source.AdaptViaReflection<SportCarDest>();

        // Assert
        result.Should().NotBeNull();
        // Base Vehicle properties
        result.Make.Should().Be(source.Make);
        result.Model.Should().Be(source.Model);
        result.Year.Should().Be(source.Year);
        // Car properties
        result.NumberOfDoors.Should().Be(source.NumberOfDoors);
        result.HasSunroof.Should().Be(source.HasSunroof);
        // SportCar properties
        result.Horsepower.Should().Be(source.Horsepower);
        result.TopSpeed.Should().Be(source.TopSpeed);
    }

    [Fact]
    public void AdaptViaReflection_InterfaceImplementation_ShouldMapAllProperties()
    {
        // Arrange
        var source = new EntitySource
        {
            Id = "entity-123",
            Name = "Test Entity",
            Description = "This is a test entity"
        };

        // Act
        var result = source.AdaptViaReflection<EntityDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(source.Id);
        result.Name.Should().Be(source.Name);
        result.Description.Should().Be(source.Description);
    }

    [Fact]
    public void AdaptViaReflection_BaseVehicle_ToBaseCar_ShouldMapCommonProperties()
    {
        // Arrange - Create a Car but reference it as Vehicle
        CarSource source = new CarSource
        {
            Make = "Honda",
            Model = "Accord",
            Year = 2023,
            NumberOfDoors = 4,
            HasSunroof = true
        };

        // Act - At runtime, it's actually a Car, so all Car properties are available
        var result = source.AdaptViaReflection<CarDest>();

        // Assert
        result.Should().NotBeNull();
        result.Make.Should().Be(source.Make);
        result.Model.Should().Be(source.Model);
        result.Year.Should().Be(source.Year);
        result.NumberOfDoors.Should().Be(source.NumberOfDoors);
    }

    [Fact]
    public void AdaptViaReflection_DifferentDerivedTypes_ShouldMapCorrectly()
    {
        // Arrange - Dog and Cat have same base but different derived properties
        var dog = new DogSource { Name = "Rex", Age = 3, Breed = "Labrador", IsGoodBoy = true };
        var cat = new CatSource { Name = "Whiskers", Age = 2, LivesRemaining = 9, LikesLasagna = false };

        // Act
        var dogResult = dog.AdaptViaReflection<DogDest>();
        var catResult = cat.AdaptViaReflection<CatDest>();

        // Assert
        dogResult.Name.Should().Be("Rex");
        dogResult.Breed.Should().Be("Labrador");
        catResult.Name.Should().Be("Whiskers");
        catResult.LivesRemaining.Should().Be(9);
    }

    [Fact]
    public void AdaptViaReflection_SportCar_WithDefaultValues_ShouldMap()
    {
        // Arrange
        var source = new SportCarSource
        {
            Make = "Unknown",
            Model = "Unknown",
            Year = 0,
            NumberOfDoors = 0,
            HasSunroof = false,
            Horsepower = 0,
            TopSpeed = 0
        };

        // Act
        var result = source.AdaptViaReflection<SportCarDest>();

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(0);
        result.Horsepower.Should().Be(0);
        result.TopSpeed.Should().Be(0);
    }

    [Fact]
    public void AdaptViaReflection_Car_ToVehicle_ShouldMapBaseProperties()
    {
        // Arrange
        var source = new CarSource
        {
            Make = "Ford",
            Model = "Mustang",
            Year = 2024,
            NumberOfDoors = 2,
            HasSunroof = true
        };

        // Act - Map to base type (but concrete VehicleDest will be created)
        var result = source.AdaptViaReflection<VehicleDest>();

        // Assert
        result.Should().NotBeNull();
        result.Make.Should().Be(source.Make);
        result.Model.Should().Be(source.Model);
        result.Year.Should().Be(source.Year);
    }

    [Fact]
    public void AdaptViaReflection_InterfaceImplementation_WithEmptyStrings_ShouldMap()
    {
        // Arrange
        var source = new EntitySource
        {
            Id = "",
            Name = "",
            Description = ""
        };

        // Act
        var result = source.AdaptViaReflection<EntityDest>();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeEmpty();
        result.Name.Should().BeEmpty();
        result.Description.Should().BeEmpty();
    }

    [Fact]
    public void AdaptViaReflection_MultipleDogs_WithCaching_ShouldMapCorrectly()
    {
        // Arrange
        var dog1 = new DogSource { Name = "Max", Age = 5, Breed = "Beagle", IsGoodBoy = true };
        var dog2 = new DogSource { Name = "Charlie", Age = 3, Breed = "Poodle", IsGoodBoy = true };
        var dog3 = new DogSource { Name = "Rocky", Age = 7, Breed = "Bulldog", IsGoodBoy = true };

        // Act - Should use cached mapper after first call
        var result1 = dog1.AdaptViaReflection<DogDest>();
        var result2 = dog2.AdaptViaReflection<DogDest>();
        var result3 = dog3.AdaptViaReflection<DogDest>();

        // Assert
        result1.Name.Should().Be("Max");
        result1.Breed.Should().Be("Beagle");
        result2.Name.Should().Be("Charlie");
        result2.Breed.Should().Be("Poodle");
        result3.Name.Should().Be("Rocky");
        result3.Breed.Should().Be("Bulldog");
    }
}

