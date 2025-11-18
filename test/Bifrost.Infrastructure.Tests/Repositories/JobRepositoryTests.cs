using Bifrost.Core.Models;
using Bifrost.Infrastructure.Persistence.Repositories;
using Bifrost.Infrastructure.Tests.Fixtures;
using FluentAssertions;

namespace Bifrost.Infrastructure.Tests.Repositories;

public class JobRepositoryTests : IClassFixture<BifrostDbContextFixture>
{
    private readonly BifrostDbContextFixture _fixture;

    public JobRepositoryTests(BifrostDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Add_ShouldAddJob()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };

        // Act
        await repository.Add(job);

        // Assert
        var result = await repository.GetById(job.Id);
        result.Should().NotBeNull();
        result.Title.Should().Be("Software Engineer");
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllJobs()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobRepository(context);
        var jobs = new List<Job>
        {
            new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() },
            new Job { Title = "Product Manager", Company = "Microsoft", SupabaseUserId = Guid.NewGuid() }
        };
        await repository.AddRange(jobs);

        // Act
        var result = await repository.GetAll();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Find_ShouldReturnMatchingJobs()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobRepository(context);
        var jobs = new List<Job>
        {
            new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() },
            new Job { Title = "Software Engineer", Company = "Microsoft", SupabaseUserId = Guid.NewGuid() }
        };
        await repository.AddRange(jobs);

        // Act
        var result = await repository.Find(j => j.Company == "Google");

        // Assert
        result.Should().HaveCount(1);
        result.First().Company.Should().Be("Google");
    }

    [Fact]
    public async Task Remove_ShouldRemoveJob()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await repository.Add(job);

        // Act
        await repository.Remove(job);

        // Assert
        var result = await repository.GetById(job.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Update_ShouldUpdateJob()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobRepository(context);
        var job = new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() };
        await repository.Add(job);
        var originalId = job.Id;

        // Act
        job.Title = "Senior Software Engineer";
        job.Company = "Microsoft";
        await repository.Update(job);

        // Assert
        var result = await repository.GetById(originalId);
        result.Should().NotBeNull();
        result.Title.Should().Be("Senior Software Engineer");
        result.Company.Should().Be("Microsoft");
    }

    [Fact]
    public async Task RemoveRange_ShouldRemoveJobs()
    {
        // Arrange
        using var context = _fixture.CreateContext();
        var repository = new JobRepository(context);
        var jobs = new List<Job>
        {
            new Job { Title = "Software Engineer", Company = "Google", SupabaseUserId = Guid.NewGuid() },
            new Job { Title = "Product Manager", Company = "Microsoft", SupabaseUserId = Guid.NewGuid() }
        };
        await repository.AddRange(jobs);

        // Act
        await repository.RemoveRange(jobs);

        // Assert
        var result = await repository.GetAll();
        result.Should().BeEmpty();
    }
}
