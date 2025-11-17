using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class ApplicationNoteRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task Add_ShouldAddApplicationNote()
    {
        // Arrange
        var dbContext = GetDbContext();
        var jobRepository = new JobRepository(dbContext);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await dbContext.SaveChangesAsync();

        var jobApplicationRepository = new JobApplicationRepository(dbContext);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await jobApplicationRepository.Add(jobApplication);
        await dbContext.SaveChangesAsync();

        var repository = new ApplicationNoteRepository(dbContext);
        var applicationNote = new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "This is a test note.", SupabaseUserId = job.SupabaseUserId };

        // Act
        await repository.Add(applicationNote);
        await dbContext.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(applicationNote.Id);
        result.Should().NotBeNull();
        result.Note.Should().Be("This is a test note.");
    }
}
