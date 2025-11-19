using System.Net.Http.Json;
using Bifrost.Contracts.Preferences;
using Bifrost.Integration.Tests.Fixtures;

namespace Bifrost.Integration.Tests.Endpoints;

/// <summary>
/// Integration tests for Preferences endpoints.
/// Tests all CRUD operations: Create (POST), Read (GET), Update (PUT), Delete (DELETE).
/// Verifies correct HTTP status codes and response formats.
/// Includes business logic validation (e.g., salary range validation).
/// </summary>
public class PreferencesEndpointsTests : IntegrationTestBase
{
    private const string PreferencesEndpoint = "/api/preferences";

    /// <summary>
    /// POST /api/preferences - Should create user preferences and return 201 Created.
    /// </summary>
    [Fact]
    public async Task CreatePreferences_WithValidData_Returns201Created()
    {
        // Arrange
        var request = new CreatePreferencesRequest(
            TestUserId,
            50000,
            150000,
            JobType.FullTime,
            true,
            false);

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync(PreferencesEndpoint, jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var result = await DeserializeResponseAsync<PreferencesResponse>(response);
        result.Should().NotBeNull();
        result!.UserId.Should().Be(TestUserId);
        result.MinSalary.Should().Be(50000);
        result.MaxSalary.Should().Be(150000);
        result.NeedSponsorship.Should().BeTrue();
        result.NeedRelocation.Should().BeFalse();
    }

    /// <summary>
    /// POST /api/preferences - Should return 400 BadRequest when user ID is empty.
    /// </summary>
    [Fact]
    public async Task CreatePreferences_WithEmptyUserId_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePreferencesRequest(
            Guid.Empty,
            50000,
            150000,
            JobType.FullTime,
            true,
            false);

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync(PreferencesEndpoint, jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// POST /api/preferences - Should return 400 BadRequest when minimum salary is greater than maximum.
    /// </summary>
    [Fact]
    public async Task CreatePreferences_WithInvalidSalaryRange_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePreferencesRequest(
            TestUserId,
            150000,  // Min > Max
            50000,
            JobType.FullTime,
            true,
            false);

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync(PreferencesEndpoint, jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// POST /api/preferences - Should return 400 BadRequest when salary values are negative.
    /// </summary>
    [Fact]
    public async Task CreatePreferences_WithNegativeSalary_Returns400BadRequest()
    {
        // Arrange
        var request = new CreatePreferencesRequest(
            TestUserId,
            -50000,
            150000,
            JobType.FullTime,
            true,
            false);

        // Act
        using var jsonContent = JsonContent.Create(request);
        var response = await Client.PostAsync(PreferencesEndpoint, jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// GET /api/preferences/user/{userId} - Should retrieve user preferences and return 200 OK.
    /// </summary>
    [Fact]
    public async Task GetUserPreferences_WithValidUserId_Returns200OkWithPreferencesData()
    {
        // Arrange
        var createRequest = new CreatePreferencesRequest(
            TestUserId,
            60000,
            120000,
            JobType.FullTime,
            false,
            true);

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync(PreferencesEndpoint, jsonContent);
        var createdPreferences = await DeserializeResponseAsync<PreferencesResponse>(createResponse);

        // Act
        var response = await Client.GetAsync($"{PreferencesEndpoint}/user/{TestUserId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PreferencesResponse>(response);
        result.Should().NotBeNull();
        result!.MinSalary.Should().Be(60000);
        result.MaxSalary.Should().Be(120000);
        result.NeedSponsorship.Should().BeFalse();
        result.NeedRelocation.Should().BeTrue();
    }

    /// <summary>
    /// GET /api/preferences/user/{userId} - Should return 404 NotFound when user has no preferences.
    /// </summary>
    [Fact]
    public async Task GetUserPreferences_WithUserHavingNoPreferences_Returns404NotFound()
    {
        // Act
        var response = await Client.GetAsync($"{PreferencesEndpoint}/user/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// PUT /api/preferences/{preferenceId} - Should update preferences and return 200 OK.
    /// </summary>
    [Fact]
    public async Task UpdatePreferences_WithValidData_Returns200OkWithUpdatedData()
    {
        // Arrange
        var createRequest = new CreatePreferencesRequest(
            TestUserId,
            40000,
            100000,
            JobType.FullTime,
            true,
            false);

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync(PreferencesEndpoint, jsonContent);
        var createdPreferences = await DeserializeResponseAsync<PreferencesResponse>(createResponse);
        var preferenceId = createdPreferences!.Id;

        var updateRequest = new UpdatePreferencesRequest(
            55000,
            130000,
            1,
            false,
            true);

        // Act
        using var updateJson = JsonContent.Create(updateRequest);
        var response = await Client.PutAsync($"{PreferencesEndpoint}/{preferenceId}", updateJson);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var result = await DeserializeResponseAsync<PreferencesResponse>(response);
        result.Should().NotBeNull();
        result!.MinSalary.Should().Be(55000);
        result.MaxSalary.Should().Be(130000);
        result.NeedSponsorship.Should().BeFalse();
        result.NeedRelocation.Should().BeTrue();
    }

    /// <summary>
    /// PUT /api/preferences/{preferenceId} - Should return 404 NotFound for non-existent preferences.
    /// </summary>
    [Fact]
    public async Task UpdatePreferences_WithNonExistentPreferenceId_Returns404NotFound()
    {
        // Arrange
        var updateRequest = new UpdatePreferencesRequest(
            50000,
            100000,
            1,
            true,
            false);

        // Act
        using var jsonContent = JsonContent.Create(updateRequest);
        var response = await Client.PutAsync($"{PreferencesEndpoint}/99999", jsonContent);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// PUT /api/preferences/{preferenceId} - Should return 400 BadRequest when updating with invalid salary range.
    /// </summary>
    [Fact]
    public async Task UpdatePreferences_WithInvalidSalaryRange_Returns400BadRequest()
    {
        // Arrange
        var createRequest = new CreatePreferencesRequest(
            TestUserId,
            50000,
            100000,
            JobType.FullTime,
            true,
            false);

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync(PreferencesEndpoint, jsonContent);
        var createdPreferences = await DeserializeResponseAsync<PreferencesResponse>(createResponse);
        var preferenceId = createdPreferences!.Id;

        var updateRequest = new UpdatePreferencesRequest(
            150000,  // Min > Max
            50000,
            1,
            true,
            false);

        // Act
        using var updateJson = JsonContent.Create(updateRequest);
        var response = await Client.PutAsync($"{PreferencesEndpoint}/{preferenceId}", updateJson);

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// DELETE /api/preferences/{preferenceId} - Should delete preferences and return 204 NoContent.
    /// </summary>
    [Fact]
    public async Task DeletePreferences_WithValidPreferenceId_Returns204NoContent()
    {
        // Arrange
        var createRequest = new CreatePreferencesRequest(
            TestUserId,
            50000,
            100000,
            JobType.FullTime,
            true,
            false);

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync(PreferencesEndpoint, jsonContent);
        var createdPreferences = await DeserializeResponseAsync<PreferencesResponse>(createResponse);
        var preferenceId = createdPreferences!.Id;

        // Act
        var response = await Client.DeleteAsync($"{PreferencesEndpoint}/{preferenceId}");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NoContent);

        // Verify preferences are actually deleted
        var getResponse = await Client.GetAsync($"{PreferencesEndpoint}/user/{TestUserId}");
        getResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// DELETE /api/preferences/{preferenceId} - Should return 404 NotFound for non-existent preferences.
    /// </summary>
    [Fact]
    public async Task DeletePreferences_WithNonExistentPreferenceId_Returns404NotFound()
    {
        // Act
        var response = await Client.DeleteAsync($"{PreferencesEndpoint}/99999");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Test that different users' preferences are isolated from each other.
    /// </summary>
    [Fact]
    public async Task GetUserPreferences_WithMultipleUsers_ReturnsOnlyUserSpecificPreferences()
    {
        // Arrange
        var user1Prefs = new CreatePreferencesRequest(
            TestUserId,
            40000,
            100000,
            JobType.FullTime,
            true,
            false);

        var user2Prefs = new CreatePreferencesRequest(
            AnotherTestUserId,
            60000,
            150000,
            JobType.FullTime,
            false,
            true);

        using var json1 = JsonContent.Create(user1Prefs);
        using var json2 = JsonContent.Create(user2Prefs);

        await Client.PostAsync(PreferencesEndpoint, json1);
        await Client.PostAsync(PreferencesEndpoint, json2);

        // Act
        var user1Response = await Client.GetAsync($"{PreferencesEndpoint}/user/{TestUserId}");
        var user2Response = await Client.GetAsync($"{PreferencesEndpoint}/user/{AnotherTestUserId}");

        // Assert
        var result1 = await DeserializeResponseAsync<PreferencesResponse>(user1Response);
        var result2 = await DeserializeResponseAsync<PreferencesResponse>(user2Response);

        result1.Should().NotBeNull();
        result1!.MinSalary.Should().Be(40000);
        result1.MaxSalary.Should().Be(100000);

        result2.Should().NotBeNull();
        result2!.MinSalary.Should().Be(60000);
        result2!.MaxSalary.Should().Be(150000);
    }

    /// <summary>
    /// Test that user can update their preferences multiple times.
    /// </summary>
    [Fact]
    public async Task UpdatePreferences_MultipleUpdates_EachUpdateSucceeds()
    {
        // Arrange
        var createRequest = new CreatePreferencesRequest(
            TestUserId,
            50000,
            100000,
            JobType.FullTime,
            true,
            false);

        using var jsonContent = JsonContent.Create(createRequest);
        var createResponse = await Client.PostAsync(PreferencesEndpoint, jsonContent);
        var createdPreferences = await DeserializeResponseAsync<PreferencesResponse>(createResponse);
        var preferenceId = createdPreferences!.Id;

        // Act & Assert - First update
        var update1 = new UpdatePreferencesRequest(60000, 120000, 1, false, true);
        using var json1 = JsonContent.Create(update1);
        var response1 = await Client.PutAsync($"{PreferencesEndpoint}/{preferenceId}", json1);
        response1.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var result1 = await DeserializeResponseAsync<PreferencesResponse>(response1);
        result1!.MinSalary.Should().Be(60000);
        result1.MaxSalary.Should().Be(120000);

        // Act & Assert - Second update
        var update2 = new UpdatePreferencesRequest(70000, 130000, 2, true, false);
        using var json2 = JsonContent.Create(update2);
        var response2 = await Client.PutAsync($"{PreferencesEndpoint}/{preferenceId}", json2);
        response2.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var result2 = await DeserializeResponseAsync<PreferencesResponse>(response2);
        result2!.MinSalary.Should().Be(70000);
        result2.MaxSalary.Should().Be(130000);
        result2.NeedSponsorship.Should().BeTrue();
    }
}
