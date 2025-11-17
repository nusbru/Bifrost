using Bifrost.Core.Models;
using Bifrost.Core.Repositories;
using Bifrost.Core.Services;
using FluentAssertions;
using NSubstitute;

namespace Bifrost.Core.Tests.Services;

public class JobApplicationServiceTests
{
    private readonly IJobApplicationRepository _applicationRepositoryMock;
    private readonly IJobRepository _jobRepositoryMock;
    private readonly JobApplicationService _jobApplicationService;

    public JobApplicationServiceTests()
    {
        _applicationRepositoryMock = Substitute.For<IJobApplicationRepository>();
        _jobRepositoryMock = Substitute.For<IJobRepository>();
        _jobApplicationService = new JobApplicationService(_applicationRepositoryMock, _jobRepositoryMock);
    }

    [Fact]
    public async Task CreateApplicationAsync_WithValidData_CreatesApplicationSuccessfully()
    {
        // Arrange
        var jobId = 1L;
        var userId = Guid.NewGuid();
        var job = new Job { Id = jobId, Title = "Test Job" };
        _jobRepositoryMock.GetById(jobId).Returns(job);

        // Act
        var result = await _jobApplicationService.CreateApplicationAsync(jobId, userId);

        // Assert
        result.Should().NotBeNull();
        result.JobId.Should().Be(jobId);
        result.SupabaseUserId.Should().Be(userId);
        result.Status.Should().Be(JobApplicationStatus.Applied);
        result.Created.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Updated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        await _applicationRepositoryMock.Received(1).Add(Arg.Any<JobApplication>());
    }

    [Fact]
    public async Task CreateApplicationAsync_WithInvalidJobId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobApplicationService.CreateApplicationAsync(0, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateApplicationAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobApplicationService.CreateApplicationAsync(1, Guid.Empty));
    }

    [Fact]
    public async Task CreateApplicationAsync_WithNonExistentJob_ThrowsInvalidOperationException()
    {
        // Arrange
        _jobRepositoryMock.GetById(999).Returns((Job?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _jobApplicationService.CreateApplicationAsync(999, Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_WithValidData_UpdatesStatusSuccessfully()
    {
        // Arrange
        var application = new JobApplication
        {
            Id = 1,
            Status = JobApplicationStatus.Applied,
            Updated = DateTime.UtcNow
        };
        _applicationRepositoryMock.GetById(1).Returns(application);

        // Act
        var result = await _jobApplicationService.UpdateApplicationStatusAsync(1, JobApplicationStatus.InProcess);

        // Assert
        result.Status.Should().Be(JobApplicationStatus.InProcess);
        result.Updated.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_WithInvalidApplicationId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobApplicationService.UpdateApplicationStatusAsync(0, JobApplicationStatus.Applied));
    }

    [Fact]
    public async Task UpdateApplicationStatusAsync_WithNonExistentApplication_ThrowsInvalidOperationException()
    {
        // Arrange
        _applicationRepositoryMock.GetById(999).Returns((JobApplication?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _jobApplicationService.UpdateApplicationStatusAsync(999, JobApplicationStatus.Applied));
    }

    [Fact]
    public async Task DeleteApplicationAsync_WithValidApplicationId_DeletesSuccessfully()
    {
        // Arrange
        var application = new JobApplication { Id = 1 };
        _applicationRepositoryMock.GetById(1).Returns(application);

        // Act
        await _jobApplicationService.DeleteApplicationAsync(1);

        // Assert
        _applicationRepositoryMock.Received(1).Remove(application);
    }

    [Fact]
    public async Task DeleteApplicationAsync_WithNonExistentApplication_ThrowsInvalidOperationException()
    {
        // Arrange
        _applicationRepositoryMock.GetById(999).Returns((JobApplication?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _jobApplicationService.DeleteApplicationAsync(999));
    }

    [Fact]
    public async Task GetApplicationByIdAsync_WithValidApplicationId_ReturnsApplication()
    {
        // Arrange
        var application = new JobApplication { Id = 1 };
        _applicationRepositoryMock.GetById(1).Returns(application);

        // Act
        var result = await _jobApplicationService.GetApplicationByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(application);
    }

    [Fact]
    public async Task GetApplicationByIdAsync_WithNonExistentApplication_ReturnsNull()
    {
        // Arrange
        _applicationRepositoryMock.GetById(999).Returns((JobApplication?)null);

        // Act
        var result = await _jobApplicationService.GetApplicationByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserApplicationsAsync_WithValidUserId_ReturnsUserApplications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applications = new List<JobApplication>
        {
            new JobApplication { Id = 1, SupabaseUserId = userId },
            new JobApplication { Id = 2, SupabaseUserId = userId }
        };
        _applicationRepositoryMock.Find(Arg.Any<System.Linq.Expressions.Expression<Func<JobApplication, bool>>>())
            .Returns(applications);

        // Act
        var result = await _jobApplicationService.GetUserApplicationsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.SupabaseUserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetUserApplicationsAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobApplicationService.GetUserApplicationsAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetJobApplicationsAsync_WithValidJobId_ReturnsJobApplications()
    {
        // Arrange
        var jobId = 1L;
        var applications = new List<JobApplication>
        {
            new JobApplication { Id = 1, JobId = jobId },
            new JobApplication { Id = 2, JobId = jobId }
        };
        _applicationRepositoryMock.Find(Arg.Any<System.Linq.Expressions.Expression<Func<JobApplication, bool>>>())
            .Returns(applications);

        // Act
        var result = await _jobApplicationService.GetJobApplicationsAsync(jobId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(a => a.JobId.Should().Be(jobId));
    }

    [Fact]
    public async Task GetJobApplicationsAsync_WithInvalidJobId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _jobApplicationService.GetJobApplicationsAsync(0));
    }
}
