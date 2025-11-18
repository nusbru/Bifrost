using Bifrost.Core.Enums;
using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using FluentAssertions;

using Bifrost.Infrastructure.Tests.Fixtures;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class PreferencesRepositoryTests : IClassFixture<BifrostDbContextFixture>
{
    private readonly BifrostDbContextFixture _fixture;

    public PreferencesRepositoryTests(BifrostDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_ShouldAddPreferences()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PreferencesRepository(context);
        var preferences = new Preferences
        {
            JobType = JobType.FullTime,
            SalaryRange = new SalaryRange { Min = 100000, Max = 150000 },
            NeedSponsorship = true,
            NeedRelocation = false,
            SupabaseUserId = Guid.NewGuid()
        };

        // Act
        await repository.Add(preferences);

        // Assert
        var result = await repository.GetById(preferences.Id);
        result.Should().NotBeNull();
        result.JobType.Should().Be(JobType.FullTime);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllPreferences()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PreferencesRepository(context);
        var preferences = new List<Preferences>
        {
            new Preferences { JobType = JobType.FullTime, SalaryRange = new SalaryRange { Min = 100000, Max = 150000 }, NeedSponsorship = true, NeedRelocation = false, SupabaseUserId = Guid.NewGuid() },
            new Preferences { JobType = JobType.PartTime, SalaryRange = new SalaryRange { Min = 50000, Max = 75000 }, NeedSponsorship = false, NeedRelocation = true, SupabaseUserId = Guid.NewGuid() }
        };
        await repository.AddRange(preferences);

        // Act
        var result = await repository.GetAll();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Find_ShouldReturnMatchingPreferences()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PreferencesRepository(context);
        var preferences = new List<Preferences>
        {
            new Preferences { JobType = JobType.FullTime, SalaryRange = new SalaryRange { Min = 100000, Max = 150000 }, NeedSponsorship = true, NeedRelocation = false, SupabaseUserId = Guid.NewGuid() },
            new Preferences { JobType = JobType.PartTime, SalaryRange = new SalaryRange { Min = 50000, Max = 75000 }, NeedSponsorship = false, NeedRelocation = true, SupabaseUserId = Guid.NewGuid() }
        };
        await repository.AddRange(preferences);

        // Act
        var result = await repository.Find(p => p.JobType == JobType.FullTime);

        // Assert
        result.Should().HaveCount(1);
        result.First().JobType.Should().Be(JobType.FullTime);
    }

    [Fact]
    public async Task Remove_ShouldRemovePreferences()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PreferencesRepository(context);
        var preferences = new Preferences { JobType = JobType.FullTime, SalaryRange = new SalaryRange { Min = 100000, Max = 150000 }, NeedSponsorship = true, NeedRelocation = false, SupabaseUserId = Guid.NewGuid() };
        await repository.Add(preferences);

        // Act
        await repository.Remove(preferences);

        // Assert
        var result = await repository.GetById(preferences.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRange_ShouldRemovePreferences()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new PreferencesRepository(context);
        var preferences = new List<Preferences>
        {
            new Preferences { JobType = JobType.FullTime, SalaryRange = new SalaryRange { Min = 100000, Max = 150000 }, NeedSponsorship = true, NeedRelocation = false, SupabaseUserId = Guid.NewGuid() },
            new Preferences { JobType = JobType.PartTime, SalaryRange = new SalaryRange { Min = 50000, Max = 75000 }, NeedSponsorship = false, NeedRelocation = true, SupabaseUserId = Guid.NewGuid() }
        };
        await repository.AddRange(preferences);

        // Act
        await repository.RemoveRange(preferences);

        // Assert
        var result = await repository.GetAll();
        result.Should().BeEmpty();
    }
}
