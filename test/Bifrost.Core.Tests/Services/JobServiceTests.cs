using Bifrost.Core.Enums;
using Bifrost.Core.Models;
using Bifrost.Core.Repositories;
using Bifrost.Core.Services;
using FluentAssertions;
using NSubstitute;

namespace Bifrost.Core.Tests.Services;

public class JobServiceTests
{
    private readonly IJobRepository _jobRepositoryMock;
    private readonly JobService _jobService;

    public JobServiceTests()
    {
        _jobRepositoryMock = Substitute.For<IJobRepository>();
        _jobService = new JobService(_jobRepositoryMock);
    }

    [Fact]
    public async Task CreateJobAsync_WithValidData_CreatesJobSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var title = "Senior Developer";
        var company = "Google";
        var location = "New York";
        var jobType = (int)JobType.FullTime;
        var description = "Exciting role";
        const bool offerSponsorship = true;
        const bool offerRelocation = false;

        // Act
        var result = await _jobService.CreateJobAsync(userId, title, company, location, 
            jobType, description, offerSponsorship, offerRelocation);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(title);
        result.Company.Should().Be(company);
        result.Location.Should().Be(location);
        result.JobType.Should().Be((JobType)jobType);
        result.Description.Should().Be(description);
        result.SupabaseUserId.Should().Be(userId);
        result.OfferSponsorship.Should().Be(offerSponsorship);
        result.OfferRelocation.Should().Be(offerRelocation);

        await _jobRepositoryMock.Received(1).Add(Arg.Any<Job>());
    }

    [Fact]
    public async Task CreateJobAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobService.CreateJobAsync(Guid.Empty, "Title", "Company", "Loc", 0, "Desc", true, true));
    }

    [Fact]
    public async Task CreateJobAsync_WithNullTitle_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobService.CreateJobAsync(Guid.NewGuid(), "", "Company", "Loc", 0, "Desc", true, true));
    }

    [Fact]
    public async Task CreateJobAsync_WithNullCompany_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobService.CreateJobAsync(Guid.NewGuid(), "Title", "", "Loc", 0, "Desc", true, true));
    }

    [Fact]
    public async Task UpdateJobAsync_WithValidData_UpdatesJobSuccessfully()
    {
        // Arrange
        var existingJob = new Job 
        { 
            Id = 1,
            Title = "Old Title",
            Company = "Old Company",
            Location = "Old Location",
            Description = "Old Desc"
        };
        _jobRepositoryMock.GetById(1).Returns(existingJob);

        // Act
        var result = await _jobService.UpdateJobAsync(1, "New Title", "New Company", "New Location", "New Desc");

        // Assert
        result.Title.Should().Be("New Title");
        result.Company.Should().Be("New Company");
        result.Location.Should().Be("New Location");
        result.Description.Should().Be("New Desc");
    }

    [Fact]
    public async Task UpdateJobAsync_WithInvalidJobId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobService.UpdateJobAsync(0, "Title", "Company", "Loc", "Desc"));
    }

    [Fact]
    public async Task UpdateJobAsync_WithNonExistentJob_ThrowsInvalidOperationException()
    {
        // Arrange
        _jobRepositoryMock.GetById(999).Returns((Job?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _jobService.UpdateJobAsync(999, "Title", "Company", "Loc", "Desc"));
    }

    [Fact]
    public async Task DeleteJobAsync_WithValidJobId_DeletesSuccessfully()
    {
        // Arrange
        var job = new Job { Id = 1, Title = "Test Job" };
        _jobRepositoryMock.GetById(1).Returns(job);

        // Act
        await _jobService.DeleteJobAsync(1);

        // Assert
        _jobRepositoryMock.Received(1).Remove(job);
    }

    [Fact]
    public async Task DeleteJobAsync_WithNonExistentJob_ThrowsInvalidOperationException()
    {
        // Arrange
        _jobRepositoryMock.GetById(999).Returns((Job?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _jobService.DeleteJobAsync(999));
    }

    [Fact]
    public async Task GetJobByIdAsync_WithValidJobId_ReturnsJob()
    {
        // Arrange
        var job = new Job { Id = 1, Title = "Test Job" };
        _jobRepositoryMock.GetById(1).Returns(job);

        // Act
        var result = await _jobService.GetJobByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(job);
    }

    [Fact]
    public async Task GetJobByIdAsync_WithNonExistentJob_ReturnsNull()
    {
        // Arrange
        _jobRepositoryMock.GetById(999).Returns((Job?)null);

        // Act
        var result = await _jobService.GetJobByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserJobsAsync_WithValidUserId_ReturnsUserJobs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobs = new List<Job>
        {
            new Job { Id = 1, Title = "Job 1", SupabaseUserId = userId },
            new Job { Id = 2, Title = "Job 2", SupabaseUserId = userId }
        };
        _jobRepositoryMock.Find(Arg.Any<System.Linq.Expressions.Expression<Func<Job, bool>>>())
            .Returns(jobs);

        // Act
        var result = await _jobService.GetUserJobsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(j => j.SupabaseUserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetUserJobsAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobService.GetUserJobsAsync(Guid.Empty));
    }
}
