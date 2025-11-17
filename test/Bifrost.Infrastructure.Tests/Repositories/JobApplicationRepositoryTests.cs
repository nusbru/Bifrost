using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class JobApplicationRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task Add_ShouldAddJobApplication()
    {
        // Arrange
        var dbContext = GetDbContext();
        var jobRepository = new JobRepository(dbContext);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await dbContext.SaveChangesAsync();

        var repository = new JobApplicationRepository(dbContext);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };

        // Act
        await repository.Add(jobApplication);
        await dbContext.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(jobApplication.Id);
        result.Should().NotBeNull();
        result.Status.Should().Be(JobApplicationStatus.Applied);
    }
}
