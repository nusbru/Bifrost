using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using Bifrost.Infrastructure.Tests.Fixtures;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class JobApplicationRepositoryTests : IClassFixture<BifrostDbContextFixture>
{
    private readonly BifrostDbContextFixture _fixture;

    public JobApplicationRepositoryTests(BifrostDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_ShouldAddJobApplication()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobApplicationRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        var jobApplication = new JobApplication { Job = job, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };

        // Act
        await repository.Add(jobApplication);
        await context.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(jobApplication.Id);
        result.Should().NotBeNull();
        result.Status.Should().Be(JobApplicationStatus.Applied);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllJobApplications()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobApplicationRepository(context);
        var userId = Guid.NewGuid();
        var job1 = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = userId };
        var job2 = new Job { Title = "DevOps Engineer", Company = "Amazon", SupabaseUserId = userId };
        context.Jobs.Add(job1);
        context.Jobs.Add(job2);
        await context.SaveChangesAsync();

        var jobApplications = new List<JobApplication>
        {
            new JobApplication { JobId = job1.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = userId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow },
            new JobApplication { JobId = job2.Id, Status = JobApplicationStatus.InProcess, SupabaseUserId = userId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow }
        };
        await repository.AddRange(jobApplications);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAll();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Find_ShouldReturnMatchingJobApplications()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobApplicationRepository(context);
        var userId = Guid.NewGuid();
        var job1 = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = userId };
        var job2 = new Job { Title = "DevOps Engineer", Company = "Amazon", SupabaseUserId = userId };
        context.Jobs.Add(job1);
        context.Jobs.Add(job2);
        await context.SaveChangesAsync();

        var jobApplications = new List<JobApplication>
        {
            new JobApplication { JobId = job1.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = userId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow },
            new JobApplication { JobId = job2.Id, Status = JobApplicationStatus.InProcess, SupabaseUserId = userId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow }
        };
        await repository.AddRange(jobApplications);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.Find(ja => ja.Status == JobApplicationStatus.Applied);

        // Assert
        result.Should().HaveCount(1);
        result.First().Status.Should().Be(JobApplicationStatus.Applied);
    }

    [Fact]
    public async Task Remove_ShouldRemoveJobApplication()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobApplicationRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        var jobApplication = new JobApplication { Job = job, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow };
        await repository.Add(jobApplication);
        await context.SaveChangesAsync();

        // Act
        repository.Remove(jobApplication);
        await context.SaveChangesAsync();

        // Assert
        var result = await repository.GetById(jobApplication.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveRange_ShouldRemoveJobApplications()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobApplicationRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        context.Jobs.Add(job);
        await context.SaveChangesAsync();

        var jobApplications = new List<JobApplication>
        {
            new JobApplication { JobId = job.Id, Status = JobApplicationStatus.Applied, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow },
            new JobApplication { JobId = job.Id, Status = JobApplicationStatus.InProcess, SupabaseUserId = job.SupabaseUserId, Created = DateTime.UtcNow, Updated = DateTime.UtcNow }
        };
        await repository.AddRange(jobApplications);
        await context.SaveChangesAsync();

        // Act
        var applicationsToRemove = await repository.GetAll();
        repository.RemoveRange(applicationsToRemove);
        await context.SaveChangesAsync();

        // Assert
        var result = await repository.GetAll();
        result.Should().BeEmpty();
    }
}
