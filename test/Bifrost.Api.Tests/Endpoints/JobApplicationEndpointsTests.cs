using Bifrost.Api.Endpoints;
using Bifrost.Contracts.JobApplications;
using Bifrost.Core.Models;
using Bifrost.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bifrost.Api.Tests.Endpoints;

public class JobApplicationEndpointsTests
{
    private readonly IJobApplicationService _applicationServiceMock;
    private readonly WebApplication _app;

    public JobApplicationEndpointsTests()
    {
        _applicationServiceMock = Substitute.For<IJobApplicationService>();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<IJobApplicationService>(_ => _applicationServiceMock);

        _app = builder.Build();
        _app.MapJobApplicationEndpoints();
    }

    [Fact]
    public async Task CreateApplication_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var jobId = 1L;
        var userId = Guid.NewGuid();
        var request = new CreateJobApplicationRequest(jobId);

        var application = new JobApplication
        {
            Id = 1,
            JobId = jobId,
            SupabaseUserId = userId,
            Status = JobApplicationStatus.Applied
        };

        _applicationServiceMock.CreateApplicationAsync(jobId, userId)
            .Returns(application);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .CreateApplicationAsync(jobId, userId);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(1);
        response.JobId.Should().Be(jobId);
        response.Status.Should().Be(JobApplicationStatus.Applied);
    }

    [Fact]
    public async Task CreateApplication_WithInvalidJobId_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _applicationServiceMock.CreateApplicationAsync(0, userId)
            .Returns(Task.FromException<JobApplication>(new ArgumentException("Job ID must be greater than zero")));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _app.Services
                .GetRequiredService<IJobApplicationService>()
                .CreateApplicationAsync(0, userId));
    }

    [Fact]
    public async Task CreateApplication_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        _applicationServiceMock.CreateApplicationAsync(1, Guid.Empty)
            .Returns(Task.FromException<JobApplication>(new ArgumentException("User ID cannot be empty")));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _app.Services
                .GetRequiredService<IJobApplicationService>()
                .CreateApplicationAsync(1, Guid.Empty));
    }

    [Fact]
    public async Task CreateApplication_WithNonExistentJob_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _applicationServiceMock.CreateApplicationAsync(999, userId)
            .Returns(Task.FromException<JobApplication>(new InvalidOperationException("Job not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IJobApplicationService>()
                .CreateApplicationAsync(999, userId));
    }

    [Fact]
    public async Task GetApplication_WithValidId_ReturnsApplication()
    {
        // Arrange
        var applicationId = 1L;
        var application = new JobApplication
        {
            Id = applicationId,
            JobId = 1,
            SupabaseUserId = Guid.NewGuid(),
            Status = JobApplicationStatus.Applied
        };

        _applicationServiceMock.GetApplicationByIdAsync(applicationId).Returns(application);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .GetApplicationByIdAsync(applicationId);

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be(applicationId);
        response.Status.Should().Be(JobApplicationStatus.Applied);
    }

    [Fact]
    public async Task GetApplication_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _applicationServiceMock.GetApplicationByIdAsync(999).Returns((JobApplication?)null);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .GetApplicationByIdAsync(999);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetUserApplications_WithValidUserId_ReturnsApplicationsList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var applications = new List<JobApplication>
        {
            new JobApplication { Id = 1, JobId = 1, SupabaseUserId = userId, Status = JobApplicationStatus.Applied },
            new JobApplication { Id = 2, JobId = 2, SupabaseUserId = userId, Status = JobApplicationStatus.InProcess }
        };

        _applicationServiceMock.GetUserApplicationsAsync(userId).Returns(applications);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .GetUserApplicationsAsync(userId);

        // Assert
        response.Should().NotBeNull();
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetJobApplications_WithValidJobId_ReturnsApplicationsList()
    {
        // Arrange
        var jobId = 1L;
        var applications = new List<JobApplication>
        {
            new JobApplication { Id = 1, JobId = jobId, Status = JobApplicationStatus.Applied },
            new JobApplication { Id = 2, JobId = jobId, Status = JobApplicationStatus.Failed }
        };

        _applicationServiceMock.GetJobApplicationsAsync(jobId).Returns(applications);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .GetJobApplicationsAsync(jobId);

        // Assert
        response.Should().NotBeNull();
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateApplicationStatus_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var applicationId = 1L;
        var request = new UpdateJobApplicationStatusRequest((int)JobApplicationStatus.InProcess);
        var application = new JobApplication
        {
            Id = applicationId,
            JobId = 1,
            Status = JobApplicationStatus.InProcess
        };

        _applicationServiceMock.UpdateApplicationStatusAsync(applicationId, JobApplicationStatus.InProcess)
            .Returns(application);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .UpdateApplicationStatusAsync(applicationId, JobApplicationStatus.InProcess);

        // Assert
        response.Should().NotBeNull();
        response.Status.Should().Be(JobApplicationStatus.InProcess);
    }

    [Fact]
    public async Task UpdateApplicationStatus_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _applicationServiceMock.UpdateApplicationStatusAsync(999, Arg.Any<JobApplicationStatus>())
            .Returns(Task.FromException<JobApplication>(new InvalidOperationException("Application not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IJobApplicationService>()
                .UpdateApplicationStatusAsync(999, JobApplicationStatus.Applied));
    }

    [Fact]
    public async Task DeleteApplication_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var applicationId = 1L;
        _applicationServiceMock.DeleteApplicationAsync(applicationId).Returns(Task.CompletedTask);

        // Act
        await _app.Services
            .GetRequiredService<IJobApplicationService>()
            .DeleteApplicationAsync(applicationId);

        // Assert
        await _applicationServiceMock.Received(1).DeleteApplicationAsync(applicationId);
    }

    [Fact]
    public async Task DeleteApplication_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _applicationServiceMock.DeleteApplicationAsync(999)
            .Returns(Task.FromException<int>(new InvalidOperationException("Application not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IJobApplicationService>()
                .DeleteApplicationAsync(999));
    }
}
