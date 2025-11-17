using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class JobRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task Add_ShouldAddJob()
    {
        // Arrange
        var dbContext = GetDbContext();
        var repository = new JobRepository(dbContext);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };

        // Act
        await repository.Add(job);
        await dbContext.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(job.Id);
        result.Should().NotBeNull();
        result.Title.Should().Be("Software Engineer");
    }
}
