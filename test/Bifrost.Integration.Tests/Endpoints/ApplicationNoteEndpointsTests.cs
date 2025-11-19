using System.Net.Http.Json;
using Bifrost.Contracts.ApplicationNotes;
using Bifrost.Contracts.Jobs;
using Bifrost.Contracts.JobApplications;
using Bifrost.Integration.Tests.Fixtures;

namespace Bifrost.Integration.Tests.Endpoints;

/// <summary>
/// Integration tests for ApplicationNote endpoints.
/// Tests all CRUD operations: Create (POST), Read (GET), Update (PUT), Delete (DELETE).
/// Verifies correct HTTP status codes and response formats.
/// Includes dependency tests (e.g., cannot create note for non-existent application).
/// </summary>
public class ApplicationNoteEndpointsTests : IntegrationTestBase
{
    private const string JobsEndpoint = "/api/jobs";
    private const string ApplicationsEndpoint = "/api/applications";

    /// <summary>
    /// Creates a test job and application and returns the application ID.
    /// </summary>
    private async Task<long> CreateTestApplicationAsync()
    {
        // Create job
        var jobRequest = new CreateJobRequest(
            TestUserId,
            "Test Position",
            "Test Company",
            "Test Location",
            0,
            "Test Description",
            true,
            false);

        using var jobJson = JsonContent.Create(jobRequest);
        var jobResponse = await Client.PostAsync(JobsEndpoint, jobJson);
        var job = await DeserializeResponseAsync<JobResponse>(jobResponse);

        // Create application
        var appRequest = new CreateJobApplicationRequest(TestUserId, job!.Id);
        using var appJson = JsonContent.Create(appRequest);
        var appResponse = await Client.PostAsync(ApplicationsEndpoint, appJson);
        var application = await DeserializeResponseAsync<JobApplicationResponse>(appResponse);

        return application!.Id;
    }

    /// <summary>
    /// POST /api/applications/{applicationId}/notes - Should create a note and return 201 Created.
    /// </summary>
    [Fact]
    public async Task CreateNote_WithValidData_Returns201Created()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var request = new CreateApplicationNoteRequest(TestUserId, "This is a test note");

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await DeserializeResponseAsync<ApplicationNoteResponse>(response);
        result.Should().NotBeNull();
        result!.JobApplicationId.Should().Be(applicationId);
        result.Note.Should().Be("This is a test note");
        result.UserId.Should().Be(TestUserId);
    }

    /// <summary>
    /// POST /api/applications/{applicationId}/notes - Should return 404 NotFound when application doesn't exist.
    /// </summary>
    [Fact]
    public async Task CreateNote_WithNonExistentApplication_Returns404NotFound()
    {
        // Arrange
        var request = new CreateApplicationNoteRequest(TestUserId, "Note content");

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync($"{ApplicationsEndpoint}/99999/notes", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// POST /api/applications/{applicationId}/notes - Should return 400 BadRequest when note is empty.
    /// </summary>
    [Fact]
    public async Task CreateNote_WithEmptyNote_Returns400BadRequest()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var request = new CreateApplicationNoteRequest(TestUserId, string.Empty);

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// POST /api/applications/{applicationId}/notes - Should return 400 BadRequest when user ID is empty.
    /// </summary>
    [Fact]
    public async Task CreateNote_WithEmptyUserId_Returns400BadRequest()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var request = new CreateApplicationNoteRequest(Guid.Empty, "Note content");

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// GET /api/applications/{applicationId}/notes/{noteId} - Should retrieve a note and return 200 OK.
    /// </summary>
    [Fact]
    public async Task GetNote_WithValidNoteId_Returns200OkWithNoteData()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var createRequest = new CreateApplicationNoteRequest(TestUserId, "Test note content");

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent);
        var createdNote = await DeserializeResponseAsync<ApplicationNoteResponse>(createResponse);
        var noteId = createdNote!.Id;

        // Act
        var response = await Client.GetAsync($"{ApplicationsEndpoint}/{applicationId}/notes/{noteId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<ApplicationNoteResponse>(response);
        result.Should().NotBeNull();
        result!.Id.Should().Be(noteId);
        result.Note.Should().Be("Test note content");
    }

    /// <summary>
    /// GET /api/applications/{applicationId}/notes/{noteId} - Should return 404 NotFound for non-existent note.
    /// </summary>
    [Fact]
    public async Task GetNote_WithNonExistentNoteId_Returns404NotFound()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();

        // Act
        var response = await Client.GetAsync($"{ApplicationsEndpoint}/{applicationId}/notes/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// GET /api/applications/{applicationId}/notes - Should retrieve all notes for an application.
    /// </summary>
    [Fact]
    public async Task GetApplicationNotes_WithValidApplicationId_Returns200OkWithNotesList()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();

        var req1 = new CreateApplicationNoteRequest(TestUserId, "First note");
        var req2 = new CreateApplicationNoteRequest(TestUserId, "Second note");

        using var json1 = JsonContent.Create(req1);
        using var json2 = JsonContent.Create(req2);

        await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", json1);
        await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", json2);

        // Act
        var response = await Client.GetAsync($"{ApplicationsEndpoint}/{applicationId}/notes/");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var notes = System.Text.Json.JsonSerializer.Deserialize<List<ApplicationNoteResponse>>(jsonContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        notes.Should().NotBeNull();
        notes.Should().HaveCount(2);
        notes!.All(n => n.JobApplicationId == applicationId).Should().BeTrue();
    }

    /// <summary>
    /// GET /api/applications/{applicationId}/notes - Should return empty list when application has no notes.
    /// </summary>
    [Fact]
    public async Task GetApplicationNotes_WithNoNotes_Returns200OkWithEmptyList()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();

        // Act
        var response = await Client.GetAsync($"{ApplicationsEndpoint}/{applicationId}/notes/");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var notes = System.Text.Json.JsonSerializer.Deserialize<List<ApplicationNoteResponse>>(jsonContent,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        notes.Should().NotBeNull();
        notes.Should().HaveCount(0);
    }

    /// <summary>
    /// PUT /api/applications/{applicationId}/notes/{noteId} - Should update a note and return 200 OK.
    /// </summary>
    [Fact]
    public async Task UpdateNote_WithValidData_Returns200OkWithUpdatedData()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var createRequest = new CreateApplicationNoteRequest(TestUserId, "Original note");

        using var jsonContent1 = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent1);
        var createdNote = await DeserializeResponseAsync<ApplicationNoteResponse>(createResponse);
        var noteId = createdNote!.Id;

        var updateRequest = new UpdateApplicationNoteRequest("Updated note content");

        // Act
        using var updateJson = JsonContent.Create(updateRequest);
        var response = await Client.PutAsync($"{ApplicationsEndpoint}/{applicationId}/notes/{noteId}", updateJson);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<ApplicationNoteResponse>(response);
        result.Should().NotBeNull();
        result!.Note.Should().Be("Updated note content");
    }

    /// <summary>
    /// PUT /api/applications/{applicationId}/notes/{noteId} - Should return 404 NotFound for non-existent note.
    /// </summary>
    [Fact]
    public async Task UpdateNote_WithNonExistentNoteId_Returns404NotFound()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var updateRequest = new UpdateApplicationNoteRequest("Updated note");

        // Act
        using var jsonContent = JsonContent.Create(updateRequest);
        var response = await Client.PutAsync($"{ApplicationsEndpoint}/{applicationId}/notes/99999", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// PUT /api/applications/{applicationId}/notes/{noteId} - Should return 400 BadRequest when updating with empty note.
    /// </summary>
    [Fact]
    public async Task UpdateNote_WithEmptyNote_Returns400BadRequest()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var createRequest = new CreateApplicationNoteRequest(TestUserId, "Original note");

        using var jsonContent1 = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent1);
        var createdNote = await DeserializeResponseAsync<ApplicationNoteResponse>(createResponse);
        var noteId = createdNote!.Id;

        var updateRequest = new UpdateApplicationNoteRequest(string.Empty);

        // Act
        using var updateJson = JsonContent.Create(updateRequest);
        var response = await Client.PutAsync($"{ApplicationsEndpoint}/{applicationId}/notes/{noteId}", updateJson);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// DELETE /api/applications/{applicationId}/notes/{noteId} - Should delete a note and return 204 NoContent.
    /// </summary>
    [Fact]
    public async Task DeleteNote_WithValidNoteId_Returns204NoContent()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();
        var createRequest = new CreateApplicationNoteRequest(TestUserId, "Note to delete");

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync($"{ApplicationsEndpoint}/{applicationId}/notes", jsonContent);
        var createdNote = await DeserializeResponseAsync<ApplicationNoteResponse>(createResponse);
        var noteId = createdNote!.Id;

        // Act
        var response = await Client.DeleteAsync($"{ApplicationsEndpoint}/{applicationId}/notes/{noteId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify note is actually deleted
        var getResponse = await Client.GetAsync($"{ApplicationsEndpoint}/{applicationId}/notes/{noteId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// DELETE /api/applications/{applicationId}/notes/{noteId} - Should return 404 NotFound for non-existent note.
    /// </summary>
    [Fact]
    public async Task DeleteNote_WithNonExistentNoteId_Returns404NotFound()
    {
        // Arrange
        var applicationId = await CreateTestApplicationAsync();

        // Act
        var response = await Client.DeleteAsync($"{ApplicationsEndpoint}/{applicationId}/notes/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test that notes are properly associated with correct applications and users.
    /// </summary>
    [Fact]
    public async Task GetApplicationNotes_WithMultipleApplications_ReturnsOnlyApplicationSpecificNotes()
    {
        // Arrange
        var app1 = await CreateTestApplicationAsync();
        var app2 = await CreateTestApplicationAsync();

        var note1 = new CreateApplicationNoteRequest(TestUserId, "Note for app 1");
        var note2 = new CreateApplicationNoteRequest(TestUserId, "Note for app 2");

        using var json1 = JsonContent.Create(note1);
        using var json2 = JsonContent.Create(note2);

        await Client.PostAsync($"{ApplicationsEndpoint}/{app1}/notes", json1);
        await Client.PostAsync($"{ApplicationsEndpoint}/{app2}/notes", json2);

        // Act
        var app1Response = await Client.GetAsync($"{ApplicationsEndpoint}/{app1}/notes/");
        var app2Response = await Client.GetAsync($"{ApplicationsEndpoint}/{app2}/notes/");

        // Assert
        var json1Content = await app1Response.Content.ReadAsStringAsync();
        var notes1 = System.Text.Json.JsonSerializer.Deserialize<List<ApplicationNoteResponse>>(json1Content,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        var json2Content = await app2Response.Content.ReadAsStringAsync();
        var notes2 = System.Text.Json.JsonSerializer.Deserialize<List<ApplicationNoteResponse>>(json2Content,
            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

        notes1.Should().HaveCount(1);
        notes1![0].Note.Should().Contain("app 1");
        notes2.Should().HaveCount(1);
        notes2![0].Note.Should().Contain("app 2");
    }
}
