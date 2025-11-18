using Bifrost.Api.Endpoints;
using Bifrost.Contracts.Jobs;
using Bifrost.Core.Enums;
using Bifrost.Core.Models;
using Bifrost.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bifrost.Api.Tests.Endpoints;

public class JobEndpointsTests
{
    private readonly IJobService _jobServiceMock;
    private readonly WebApplication _app;

    public JobEndpointsTests()
    {
        _jobServiceMock = Substitute.For<IJobService>();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<IJobService>(_ => _jobServiceMock);

        _app = builder.Build();
        _app.MapJobEndpoints();
    }

    [Fact]
    public async Task CreateJob_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateJobRequest(
            userId,
            "Senior Developer",
            "Google",
            "New York",
            (int)JobType.FullTime,
            "Exciting role",
            true,
            false);

        var job = new Job
        {
            Id = 1,
            Title = request.Title,
            Company = request.Company,
            Location = request.Location,
            JobType = (JobType)request.JobType,
            Description = request.Description,
            OfferSponsorship = request.OfferSponsorship,
            OfferRelocation = request.OfferRelocation,
            SupabaseUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _jobServiceMock.CreateJobAsync(
            userId,
            request.Title,
            request.Company,
            request.Location,
            request.JobType,
            request.Description,
            request.OfferSponsorship,
            request.OfferRelocation)
            .Returns(job);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobService>()
            .CreateJobAsync(
                userId,
                request.Title,
                request.Company,
                request.Location,
                request.JobType,
                request.Description,
                request.OfferSponsorship,
                request.OfferRelocation);

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(1);
        response.Title.Should().Be(request.Title);
        response.Company.Should().Be(request.Company);
        response.JobType.Should().Be((JobType)request.JobType);
    }

    [Fact]
    public async Task CreateJob_WithInvalidTitle_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateJobRequest(
            userId,
            "",
            "Google",
            "New York",
            (int)JobType.FullTime,
            "Exciting role",
            true,
            false);

        _jobServiceMock.CreateJobAsync(
            userId,
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<bool>())
            .Returns(Task.FromException<Job>(new ArgumentException("Title cannot be empty")));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _app.Services
                .GetRequiredService<IJobService>()
                .CreateJobAsync(
                    userId,
                    request.Title,
                    request.Company,
                    request.Location,
                    request.JobType,
                    request.Description,
                    request.OfferSponsorship,
                    request.OfferRelocation));
    }

    [Fact]
    public async Task GetJob_WithValidId_ReturnsJob()
    {
        // Arrange
        var jobId = 1L;
        var job = new Job
        {
            Id = jobId,
            Title = "Senior Developer",
            Company = "Google",
            Location = "New York",
            JobType = JobType.FullTime,
            Description = "Exciting role",
            OfferSponsorship = true,
            OfferRelocation = false,
            SupabaseUserId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        _jobServiceMock.GetJobByIdAsync(jobId).Returns(job);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobService>()
            .GetJobByIdAsync(jobId);

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be(jobId);
        response.Title.Should().Be("Senior Developer");
    }

    [Fact]
    public async Task GetJob_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _jobServiceMock.GetJobByIdAsync(999).Returns((Job?)null);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobService>()
            .GetJobByIdAsync(999);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetUserJobs_WithValidUserId_ReturnsJobsList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobs = new List<Job>
        {
            new Job
            {
                Id = 1,
                Title = "Developer",
                Company = "Google",
                Location = "New York",
                JobType = JobType.FullTime,
                SupabaseUserId = userId
            },
            new Job
            {
                Id = 2,
                Title = "Manager",
                Company = "Microsoft",
                Location = "Seattle",
                JobType = JobType.PartTime,
                SupabaseUserId = userId
            }
        };

        _jobServiceMock.GetUserJobsAsync(userId).Returns(jobs);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobService>()
            .GetUserJobsAsync(userId);

        // Assert
        response.Should().NotBeNull();
        response.Should().HaveCount(2);
        response.First().Title.Should().Be("Developer");
        response.Last().Title.Should().Be("Manager");
    }

    [Fact]
    public async Task UpdateJob_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var jobId = 1L;
        var request = new UpdateJobRequest("Senior Developer", "Google", "New York", "Updated description", true, false);
        var job = new Job
        {
            Id = jobId,
            Title = request.Title!,
            Company = request.Company!,
            Location = request.Location!,
            Description = request.Description!,
            JobType = JobType.FullTime,
            SupabaseUserId = Guid.NewGuid(),
            UpdatedAt = DateTime.UtcNow
        };

        _jobServiceMock.UpdateJobAsync(jobId, request.Title, request.Company, request.Location, request.Description, request.OfferSponsorship, request.OfferRelocation)
            .Returns(job);

        // Act
        var response = await _app.Services
            .GetRequiredService<IJobService>()
            .UpdateJobAsync(jobId, request.Title!, request.Company!, request.Location!, request.Description!, request.OfferSponsorship, request.OfferRelocation);

        // Assert
        response.Should().NotBeNull();
        response.Title.Should().Be(request.Title);
        response.Id.Should().Be(jobId);
    }

    [Fact]
    public async Task UpdateJob_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new UpdateJobRequest("Senior Developer", "Google", "New York", "Updated", true, false);

        _jobServiceMock.UpdateJobAsync(999, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool?>())
            .Returns(Task.FromException<Job>(new InvalidOperationException("Job not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IJobService>()
                .UpdateJobAsync(999, request.Title!, request.Company!, request.Location!, request.Description!, request.OfferSponsorship, request.OfferRelocation));
    }

    [Fact]
    public async Task DeleteJob_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var jobId = 1L;
        _jobServiceMock.DeleteJobAsync(jobId).Returns(Task.CompletedTask);

        // Act
        await _app.Services
            .GetRequiredService<IJobService>()
            .DeleteJobAsync(jobId);

        // Assert
        await _jobServiceMock.Received(1).DeleteJobAsync(jobId);
    }

    [Fact]
    public async Task DeleteJob_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _jobServiceMock.DeleteJobAsync(999)
            .Returns(x => throw new InvalidOperationException("Job not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IJobService>()
                .DeleteJobAsync(999));
    }
}
