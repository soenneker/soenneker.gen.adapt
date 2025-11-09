using Soenneker.Tests.Unit;
using Soenneker.Gen.Adapt.Tests.Dtos;
using System;
using System.Collections.Generic;
using Xunit;
using AwesomeAssertions;

namespace Soenneker.Gen.Adapt.Tests;

public sealed class TemporalTypeTests : UnitTest
{
    public TemporalTypeTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void Adapt_TemporalProperties_ShouldMapCorrectly()
    {
        // Arrange
        var profileUri = new Uri("https://example.org/users/primary");
        var backupUri = new Uri("https://backup.example.org/users/primary");
        DateOnly today = new(2025, 11, 9);
        TimeOnly now = new(14, 30, 15);

        var source = new TemporalSourceDto
        {
            Date = today,
            Time = now,
            OptionalDate = today.AddDays(1),
            OptionalTime = now.AddHours(1),
            ProfileLink = profileUri,
            BackupLink = backupUri,
            Nested = new TemporalNestedSourceDto
            {
                EffectiveDate = today.AddDays(2),
                CutoffTime = new TimeOnly(18, 45, 0),
                Documentation = new Uri("https://docs.example.org/policies"),
                SupportLink = new Uri("https://support.example.org/contact")
            }
        };

        // Act
        var result = source.Adapt<TemporalDestDto>();

        // Assert
        result.Should().NotBeNull();
        result.Date.Should().Be(today);
        result.Time.Should().Be(now);
        result.OptionalDate.Should().Be(today.AddDays(1));
        result.OptionalTime.Should().Be(now.AddHours(1));
        result.ProfileLink.Should().Be(profileUri);
        result.BackupLink.Should().Be(backupUri);

        result.Nested.Should().NotBeNull();
        result.Nested.EffectiveDate.Should().Be(today.AddDays(2));
        result.Nested.CutoffTime.Should().Be(new TimeOnly(18, 45));
        result.Nested.Documentation.Should().Be(new Uri("https://docs.example.org/policies"));
        result.Nested.SupportLink.Should().Be(new Uri("https://support.example.org/contact"));
    }

    [Fact]
    public void Adapt_TemporalNullableProperties_ShouldMapNulls()
    {
        // Arrange
        var source = new TemporalSourceDto
        {
            OptionalDate = null,
            OptionalTime = null,
            BackupLink = null,
            ImportantDates = Array.Empty<DateOnly>(),
            MeetingTimes = [],
            ResourceLinks = Array.Empty<Uri>(),
            FavoriteLinks = [],
            NamedDates = new Dictionary<string, DateOnly?>
            {
                ["release"] = null,
                ["sunset"] = null
            },
            Nested = new TemporalNestedSourceDto
            {
                EffectiveDate = new DateOnly(2025, 01, 01),
                CutoffTime = new TimeOnly(0, 0),
                Documentation = new Uri("https://docs.example.org/default"),
                SupportLink = null
            }
        };

        // Act
        var result = source.Adapt<TemporalDestDto>();

        // Assert
        result.OptionalDate.Should().BeNull();
        result.OptionalTime.Should().BeNull();
        result.BackupLink.Should().BeNull();
        result.ImportantDates.Should().BeEmpty();
        result.MeetingTimes.Should().BeEmpty();
        result.ResourceLinks.Should().BeEmpty();
        result.FavoriteLinks.Should().BeEmpty();
        result.NamedDates.Should().HaveCount(2);
        result.NamedDates["release"].Should().BeNull();
        result.NamedDates["sunset"].Should().BeNull();
        result.Nested.SupportLink.Should().BeNull();
    }

    [Fact]
    public void Adapt_TemporalCollections_ShouldMapAndConvert()
    {
        // Arrange
        var importantDates = new[]
        {
            new DateOnly(2024, 12, 31),
            new DateOnly(2025, 01, 01),
            new DateOnly(2025, 06, 15)
        };

        var meetingTimes = new List<TimeOnly>
        {
            new(9, 0),
            new(13, 30),
            new(17, 45)
        };

        IReadOnlyList<Uri> resources = new[]
        {
            new Uri("https://example.org/resources/overview"),
            new Uri("https://example.org/resources/details")
        };

        var favoriteLinks = new HashSet<Uri>
        {
            new("https://example.org/favorites/1"),
            new("https://example.org/favorites/2")
        };

        var source = new TemporalSourceDto
        {
            ImportantDates = importantDates,
            MeetingTimes = meetingTimes,
            ResourceLinks = resources,
            FavoriteLinks = favoriteLinks,
            NamedDates = new Dictionary<string, DateOnly?>
            {
                ["kickoff"] = importantDates[0],
                ["launch"] = importantDates[1]
            }
        };

        // Act
        var result = source.Adapt<TemporalDestDto>();

        // Assert
        result.ImportantDates.Should().HaveCount(3);
        result.ImportantDates[0].Should().Be(new DateOnly(2024, 12, 31));
        result.ImportantDates.Should().BeOfType<List<DateOnly>>();

        result.MeetingTimes.Should().HaveCount(3);
        result.MeetingTimes[1].Should().Be(new TimeOnly(13, 30));

        result.ResourceLinks.Should().HaveCount(2);
        result.ResourceLinks.Should().AllSatisfy(uri => uri.Should().NotBeNull());

        result.FavoriteLinks.Should().HaveCount(2);
        result.FavoriteLinks.Should().Contain(new Uri("https://example.org/favorites/1"));

        result.NamedDates.Should().ContainKey("kickoff");
        result.NamedDates["kickoff"].Should().Be(new DateOnly(2024, 12, 31));
        result.NamedDates["launch"].Should().Be(new DateOnly(2025, 01, 01));
    }
}

