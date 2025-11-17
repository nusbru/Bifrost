using Bifrost.Api.Endpoints;
using Bifrost.Contracts.Preferences;
using Bifrost.Core.Enums;
using Bifrost.Core.Models;
using Bifrost.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bifrost.Api.Tests.Endpoints;

public class PreferencesEndpointsTests
{
    private readonly IPreferencesService _preferencesServiceMock;
    private readonly WebApplication _app;

    public PreferencesEndpointsTests()
    {
        _preferencesServiceMock = Substitute.For<IPreferencesService>();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<IPreferencesService>(_ => _preferencesServiceMock);

        _app = builder.Build();
        _app.MapPreferencesEndpoints();
    }

    [Fact]
    public async Task CreatePreferences_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreatePreferencesRequest(50000m, 100000m, "FullTime", "US");

        var preferences = new Preferences
        {
            Id = 1,
            SupabaseUserId = userId,
            JobType = JobType.FullTime,
            SalaryRange = new SalaryRange { Min = 50000, Max = 100000 },
            NeedSponsorship = false
        };

        _preferencesServiceMock.CreatePreferencesAsync(userId, 50000m, 100000m, "FullTime", "US")
            .Returns(preferences);

        // Act
        var response = await _app.Services
            .GetRequiredService<IPreferencesService>()
            .CreatePreferencesAsync(userId, 50000m, 100000m, "FullTime", "US");

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(1);
        response.SupabaseUserId.Should().Be(userId);
    }

    [Fact]
    public async Task CreatePreferences_WithInvalidSalaryRange_ThrowsArgumentException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _preferencesServiceMock
            .CreatePreferencesAsync(userId, 100000m, 50000m, "FullTime", "US")
            .Returns(Task.FromException<Preferences>(new ArgumentException("Min salary must be less than or equal to max salary")));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _app.Services
                .GetRequiredService<IPreferencesService>()
                .CreatePreferencesAsync(userId, 100000m, 50000m, "FullTime", "US"));
    }

    [Fact]
    public async Task CreatePreferences_WithInvalidUserId_ThrowsArgumentException()
    {
        // Arrange
        _preferencesServiceMock
            .CreatePreferencesAsync(Guid.Empty, 50000m, 100000m, "FullTime", "US")
            .Returns(Task.FromException<Preferences>(new ArgumentException("User ID cannot be empty")));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _app.Services
                .GetRequiredService<IPreferencesService>()
                .CreatePreferencesAsync(Guid.Empty, 50000m, 100000m, "FullTime", "US"));
    }

    [Fact]
    public async Task GetUserPreferences_WithValidUserId_ReturnsPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new Preferences
        {
            Id = 1,
            SupabaseUserId = userId,
            JobType = JobType.FullTime,
            SalaryRange = new SalaryRange { Min = 50000, Max = 100000 },
            NeedSponsorship = false
        };

        _preferencesServiceMock.GetUserPreferencesAsync(userId).Returns(preferences);

        // Act
        var response = await _app.Services
            .GetRequiredService<IPreferencesService>()
            .GetUserPreferencesAsync(userId);

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be(1);
        response.SupabaseUserId.Should().Be(userId);
        response.SalaryRange.Min.Should().Be(50000);
        response.SalaryRange.Max.Should().Be(100000);
    }

    [Fact]
    public async Task GetUserPreferences_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _preferencesServiceMock.GetUserPreferencesAsync(userId).Returns((Preferences?)null);

        // Act
        var response = await _app.Services
            .GetRequiredService<IPreferencesService>()
            .GetUserPreferencesAsync(userId);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task UpdatePreferences_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var preferencesId = 1L;
        var request = new UpdatePreferencesRequest(60000m, 120000m, "FullTime,Contract", "US,CA");

        var preferences = new Preferences
        {
            Id = preferencesId,
            SupabaseUserId = Guid.NewGuid(),
            JobType = JobType.FullTime,
            SalaryRange = new SalaryRange { Min = 60000, Max = 120000 },
            NeedSponsorship = false
        };

        _preferencesServiceMock.UpdatePreferencesAsync(preferencesId, 60000m, 120000m, "FullTime,Contract", "US,CA")
            .Returns(preferences);

        // Act
        var response = await _app.Services
            .GetRequiredService<IPreferencesService>()
            .UpdatePreferencesAsync(preferencesId, 60000m, 120000m, "FullTime,Contract", "US,CA");

        // Assert
        response.Should().NotBeNull();
        response.SalaryRange.Min.Should().Be(60000);
        response.SalaryRange.Max.Should().Be(120000);
    }

    [Fact]
    public async Task UpdatePreferences_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _preferencesServiceMock
            .UpdatePreferencesAsync(999, 50000m, 100000m, "FullTime", "US")
            .Returns(Task.FromException<Preferences>(new InvalidOperationException("Preferences not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IPreferencesService>()
                .UpdatePreferencesAsync(999, 50000m, 100000m, "FullTime", "US"));
    }

    [Fact]
    public async Task DeletePreferences_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var preferencesId = 1L;
        _preferencesServiceMock.DeletePreferencesAsync(preferencesId).Returns(Task.CompletedTask);

        // Act
        await _app.Services
            .GetRequiredService<IPreferencesService>()
            .DeletePreferencesAsync(preferencesId);

        // Assert
        await _preferencesServiceMock.Received(1).DeletePreferencesAsync(preferencesId);
    }

    [Fact]
    public async Task DeletePreferences_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _preferencesServiceMock.DeletePreferencesAsync(999)
            .Returns(x => throw new InvalidOperationException("Preferences not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IPreferencesService>()
                .DeletePreferencesAsync(999));
    }
}
