using Bifrost.Core.Enums;
using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class PreferencesRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task Add_ShouldAddPreferences()
    {
        // Arrange
        var dbContext = GetDbContext();
        var repository = new PreferencesRepository(dbContext);
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
        await dbContext.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(preferences.Id);
        result.Should().NotBeNull();
        result.JobType.Should().Be(JobType.FullTime);
    }
}
