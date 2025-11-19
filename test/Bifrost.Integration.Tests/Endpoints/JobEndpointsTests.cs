using System.Net.Http.Json;
using System.Text.Json;
using Bifrost.Contracts.Jobs;
using Bifrost.Integration.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Integration.Tests.Endpoints;

/// <summary>
/// Integration tests for Job endpoints.
/// Tests all CRUD operations: Create (POST), Read (GET), Update (PUT), Delete (DELETE).
/// Verifies correct HTTP status codes and response formats.
/// </summary>
public class JobEndpointsTests : IntegrationTestBase
{
    private const string JobsEndpoint = "/api/jobs";

    /// <summary>
    /// POST /api/jobs - Should create a new job and return 201 Created.
    /// </summary>
    [Fact]
    public async Task CreateJob_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new CreateJobRequest(
            TestUserId,
            "Senior Developer",
            "Google",
            "New York",
            0,
            "Build amazing products",
            true,
            false);

        // Act
        var response = await Client.PostAsJsonAsync(JobsEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await DeserializeResponseAsync<JobResponse>(response);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Senior Developer");
        result.Company.Should().Be("Google");
        result.Location.Should().Be("New York");
        result.OfferSponsorship.Should().BeTrue();
        result.OfferRelocation.Should().BeFalse();
        result.UserId.Should().Be(TestUserId);
    }

    /// <summary>
    /// POST /api/jobs - Should return 400 BadRequest when title is empty.
    /// </summary>
    [Fact]
    public async Task CreateJob_WithEmptyTitle_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateJobRequest(
            TestUserId,
            string.Empty,
            "Google",
            "New York",
            0,
            "Description",
            true,
            false);

        // Act
        var response = await Client.PostAsJsonAsync(JobsEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var problemDetails = await ParseProblemDetailsAsync(response);
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Validation Error");
    }

    /// <summary>
    /// POST /api/jobs - Should return 400 BadRequest when company is empty.
    /// </summary>
    [Fact]
    public async Task CreateJob_WithEmptyCompany_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateJobRequest(
            TestUserId,
            "Senior Developer",
            string.Empty,
            "New York",
            0,
            "Description",
            true,
            false);

        // Act
        var response = await Client.PostAsJsonAsync(JobsEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// POST /api/jobs - Should return 400 BadRequest when user ID is empty (Guid.Empty).
    /// </summary>
    [Fact]
    public async Task CreateJob_WithEmptyUserId_Returns400BadRequest()
    {
        // Arrange
        var request = new CreateJobRequest(
            Guid.Empty,
            "Senior Developer",
            "Google",
            "New York",
            0,
            "Description",
            true,
            false);

        // Act
        var response = await Client.PostAsJsonAsync(JobsEndpoint, request);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// GET /api/jobs/{jobId} - Should retrieve a job by ID and return 200 OK.
    /// </summary>
    [Fact]
    public async Task GetJob_WithValidJobId_Returns200OkWithJobData()
    {
        // Arrange
        var createRequest = new CreateJobRequest(
            TestUserId,
            "Developer",
            "Microsoft",
            "Seattle",
            1,
            "Build cloud solutions",
            true,
            true);

        var createResponse = await Client.PostAsJsonAsync(JobsEndpoint, createRequest);
        var createdJob = await DeserializeResponseAsync<JobResponse>(createResponse);
        var jobId = createdJob!.Id;

        // Act
        var response = await Client.GetAsync($"{JobsEndpoint}/{jobId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<JobResponse>(response);
        result.Should().NotBeNull();
        result!.Id.Should().Be(jobId);
        result.Title.Should().Be("Developer");
        result.Company.Should().Be("Microsoft");
    }

    /// <summary>
    /// GET /api/jobs/{jobId} - Should return 404 NotFound for non-existent job.
    /// </summary>
    [Fact]
    public async Task GetJob_WithNonExistentJobId_Returns404NotFound()
    {
        // Act
        var response = await Client.GetAsync($"{JobsEndpoint}/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// GET /api/jobs/user/{userId} - Should retrieve all jobs for a user.
    /// </summary>
    [Fact]
    public async Task GetUserJobs_WithValidUserId_Returns200OkWithJobsList()
    {
        // Arrange
        var request1 = new CreateJobRequest(TestUserId, "Developer", "Google", "NY", 0, "Desc", true, true);
        var request2 = new CreateJobRequest(TestUserId, "DevOps", "Amazon", "Seattle", 2, "Desc", false, true);

        await Client.PostAsJsonAsync(JobsEndpoint, request1);
        await Client.PostAsJsonAsync(JobsEndpoint, request2);

        // Act
        var response = await Client.GetAsync($"{JobsEndpoint}/user/{TestUserId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jobs = System.Text.Json.JsonSerializer.Deserialize<List<JobResponse>>(jsonContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        jobs.Should().NotBeNull();
        jobs.Should().HaveCount(2);
        jobs!.All(j => j.UserId == TestUserId).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/jobs/user/{userId} - Should return empty list when user has no jobs.
    /// </summary>
    [Fact]
    public async Task GetUserJobs_WithUserHavingNoJobs_Returns200OkWithEmptyList()
    {
        // Act
        var response = await Client.GetAsync($"{JobsEndpoint}/user/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var jobs = System.Text.Json.JsonSerializer.Deserialize<List<JobResponse>>(jsonContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        jobs.Should().NotBeNull();
        jobs.Should().HaveCount(0);
    }

    /// <summary>
    /// PUT /api/jobs/{jobId} - Should update job details and return 200 OK.
    /// </summary>
    [Fact]
    public async Task UpdateJob_WithValidData_Returns200OkWithUpdatedData()
    {
        // Arrange
        var createRequest = new CreateJobRequest(
            TestUserId,
            "Junior Developer",
            "Apple",
            "Cupertino",
            0,
            "Original description",
            true,
            false);

        var createResponse = await Client.PostAsJsonAsync(JobsEndpoint, createRequest);
        var createdJob = await DeserializeResponseAsync<JobResponse>(createResponse);
        var jobId = createdJob!.Id;

        var updateRequest = new UpdateJobRequest(
            "Senior Developer",
            "Apple",
            "San Francisco",
            "Updated description",
            false,
            true);

        // Act
        var response = await Client.PutAsJsonAsync($"{JobsEndpoint}/{jobId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<JobResponse>(response);
        result.Should().NotBeNull();
        result!.Title.Should().Be("Senior Developer");
        result.Location.Should().Be("San Francisco");
        result.Description.Should().Be("Updated description");
        result.OfferSponsorship.Should().BeFalse();
        result.OfferRelocation.Should().BeTrue();
    }

    /// <summary>
    /// PUT /api/jobs/{jobId} - Should return 404 NotFound for non-existent job.
    /// </summary>
    [Fact]
    public async Task UpdateJob_WithNonExistentJobId_Returns404NotFound()
    {
        // Arrange
        var updateRequest = new UpdateJobRequest(
            "Senior Developer",
            "Google",
            "NYC",
            "Description",
            true,
            false);

        // Act
        var response = await Client.PutAsJsonAsync($"{JobsEndpoint}/99999", updateRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// PUT /api/jobs/{jobId} - Update with empty title should return 200 OK and keep original title.
    /// Implementation silently ignores empty/whitespace values during updates.
    /// </summary>
    [Fact]
    public async Task UpdateJob_WithEmptyTitle_Returns200OkAndKeepsOriginalTitle()
    {
        // Arrange
        var createRequest = new CreateJobRequest(
            TestUserId,
            "Developer",
            "Google",
            "NYC",
            0,
            "Description",
            true,
            false);

        var createResponse = await Client.PostAsJsonAsync(JobsEndpoint, createRequest);
        var createdJob = await DeserializeResponseAsync<JobResponse>(createResponse);
        var jobId = createdJob!.Id;
        var originalTitle = createdJob.Title;

        var updateRequest = new UpdateJobRequest(
            string.Empty,
            "Amazon",  // Change company to verify update worked
            "Seattle",
            "Updated Description",
            false,
            true);

        // Act
        var response = await Client.PutAsJsonAsync($"{JobsEndpoint}/{jobId}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var updatedJob = await DeserializeResponseAsync<JobResponse>(response);
        updatedJob.Should().NotBeNull();
        updatedJob!.Title.Should().Be(originalTitle);  // Title should remain unchanged
        updatedJob.Company.Should().Be("Amazon");  // Other fields should be updated
    }

    /// <summary>
    /// DELETE /api/jobs/{jobId} - Should delete a job and return 204 NoContent.
    /// </summary>
    [Fact]
    public async Task DeleteJob_WithValidJobId_Returns204NoContent()
    {
        // Arrange
        var createRequest = new CreateJobRequest(
            TestUserId,
            "Developer",
            "Netflix",
            "Los Gatos",
            0,
            "Description",
            true,
            true);

        var createResponse = await Client.PostAsJsonAsync(JobsEndpoint, createRequest);
        var createdJob = await DeserializeResponseAsync<JobResponse>(createResponse);
        var jobId = createdJob!.Id;

        // Act
        var response = await Client.DeleteAsync($"{JobsEndpoint}/{jobId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify job is actually deleted
        var getResponse = await Client.GetAsync($"{JobsEndpoint}/{jobId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// DELETE /api/jobs/{jobId} - Should return 404 NotFound when deleting non-existent job.
    /// </summary>
    [Fact]
    public async Task DeleteJob_WithNonExistentJobId_Returns404NotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"{JobsEndpoint}/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test multi-user isolation - should only retrieve jobs for specific user.
    /// </summary>
    [Fact]
    public async Task GetUserJobs_WithMultipleUsers_ReturnsOnlyUserSpecificJobs()
    {
        // Arrange
        var user1Job = new CreateJobRequest(TestUserId, "Developer", "Google", "NYC", 0, "Desc", true, true);
        var user2Job = new CreateJobRequest(AnotherTestUserId, "Designer", "Apple", "Cupertino", 1, "Desc", false, false);

        await Client.PostAsJsonAsync(JobsEndpoint, user1Job);
        await Client.PostAsJsonAsync(JobsEndpoint, user2Job);

        // Act
        var user1Response = await Client.GetAsync($"{JobsEndpoint}/user/{TestUserId}");
        var user2Response = await Client.GetAsync($"{JobsEndpoint}/user/{AnotherTestUserId}");

        // Assert
        var jsonContent1 = await user1Response.Content.ReadAsStringAsync();
        var user1Jobs = System.Text.Json.JsonSerializer.Deserialize<List<JobResponse>>(jsonContent1,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var jsonContent2 = await user2Response.Content.ReadAsStringAsync();
        var user2Jobs = System.Text.Json.JsonSerializer.Deserialize<List<JobResponse>>(jsonContent2,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        user1Jobs.Should().HaveCount(1);
        user1Jobs![0].Title.Should().Be("Developer");
        user2Jobs.Should().HaveCount(1);
        user2Jobs![0].Title.Should().Be("Designer");
    }
}
