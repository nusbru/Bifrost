using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using Bifrost.Infrastructure.Tests.Fixtures;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class ApplicationNoteRepositoryTests : IClassFixture<BifrostDbContextFixture>
{
    private readonly BifrostDbContextFixture _fixture;

    public ApplicationNoteRepositoryTests(BifrostDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_ShouldAddApplicationNote()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var jobRepository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await context.SaveChangesAsync();

        var jobApplicationRepository = new JobApplicationRepository(context);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await jobApplicationRepository.Add(jobApplication);
        await context.SaveChangesAsync();

        var repository = new ApplicationNoteRepository(context);
        var applicationNote = new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "This is a test note.", SupabaseUserId = job.SupabaseUserId };

        // Act
        await repository.Add(applicationNote);
        await context.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(applicationNote.Id);
        result.Should().NotBeNull();
        result.Note.Should().Be("This is a test note.");
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllApplicationNotes()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var jobRepository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await context.SaveChangesAsync();

        var jobApplicationRepository = new JobApplicationRepository(context);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await jobApplicationRepository.Add(jobApplication);
        await context.SaveChangesAsync();

        var repository = new ApplicationNoteRepository(context);
        var applicationNotes = new List<ApplicationNote>
        {
            new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "Note 1", SupabaseUserId = job.SupabaseUserId },
            new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "Note 2", SupabaseUserId = job.SupabaseUserId }
        };
        await repository.AddRange(applicationNotes);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAll();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Find_ShouldReturnMatchingApplicationNotes()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var jobRepository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await context.SaveChangesAsync();

        var jobApplicationRepository = new JobApplicationRepository(context);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await jobApplicationRepository.Add(jobApplication);
        await context.SaveChangesAsync();

        var repository = new ApplicationNoteRepository(context);
        var applicationNotes = new List<ApplicationNote>
        {
            new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "Note 1", SupabaseUserId = job.SupabaseUserId },
            new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "Note 2", SupabaseUserId = job.SupabaseUserId }
        };
        await repository.AddRange(applicationNotes);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.Find(an => an.Note == "Note 1");

        // Assert
        result.Should().HaveCount(1);
        result.First().Note.Should().Be("Note 1");
    }

    [Fact]
    public async Task Remove_ShouldRemoveApplicationNote()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var jobRepository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await context.SaveChangesAsync();

        var jobApplicationRepository = new JobApplicationRepository(context);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await jobApplicationRepository.Add(jobApplication);
        await context.SaveChangesAsync();

        var repository = new ApplicationNoteRepository(context);
        var applicationNote = new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "This is a test note.", SupabaseUserId = job.SupabaseUserId };
        await repository.Add(applicationNote);
        await context.SaveChangesAsync();

        // Act
        repository.Remove(applicationNote);
        await context.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(applicationNote.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRange_ShouldRemoveApplicationNotes()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var jobRepository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await jobRepository.Add(job);
        await context.SaveChangesAsync();

        var jobApplicationRepository = new JobApplicationRepository(context);
        var jobApplication = new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await jobApplicationRepository.Add(jobApplication);
        await context.SaveChangesAsync();

        var repository = new ApplicationNoteRepository(context);
        var applicationNotes = new List<ApplicationNote>
        {
            new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "Note 1", SupabaseUserId = job.SupabaseUserId },
            new ApplicationNote { JobApplicationId = jobApplication.Id, Note = "Note 2", SupabaseUserId = job.SupabaseUserId }
        };
        await repository.AddRange(applicationNotes);
        await context.SaveChangesAsync();

        // Act
        repository.RemoveRange(applicationNotes);
        await context.SaveChangesAsync();

        // Assert
        var result = await repository.GetAll();
        result.Should().BeEmpty();
    }
}
