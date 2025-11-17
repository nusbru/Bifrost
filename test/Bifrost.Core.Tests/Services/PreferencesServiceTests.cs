using Bifrost.Core.Models;
using Bifrost.Core.Repositories;
using Bifrost.Core.Services;
using FluentAssertions;
using NSubstitute;

namespace Bifrost.Core.Tests.Services;

public class PreferencesServiceTests
{
    private readonly IPreferencesRepository _preferencesRepositoryMock;
    private readonly PreferencesService _preferencesService;

    public PreferencesServiceTests()
    {
        _preferencesRepositoryMock = Substitute.For<IPreferencesRepository>();
        _preferencesService = new PreferencesService(_preferencesRepositoryMock);
    }

    [Fact]
    public async Task CreatePreferencesAsync_WithValidData_CreatesPreferencesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var minSalary = 50000m;
        var maxSalary = 100000m;
        var jobTypes = "FullTime,PartTime";
        var locations = "New York,Remote";

        // Act
        var result = await _preferencesService.CreatePreferencesAsync(
            userId, minSalary, maxSalary, jobTypes, locations);

        // Assert
        result.Should().NotBeNull();
        result.SupabaseUserId.Should().Be(userId);
        result.SalaryRange.Min.Should().Be(minSalary);
        result.SalaryRange.Max.Should().Be(maxSalary);

        await _preferencesRepositoryMock.Received(1).Add(Arg.Any<Preferences>());
    }

    [Fact]
    public async Task CreatePreferencesAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.CreatePreferencesAsync(Guid.Empty, 50000, 100000, "", ""));
    }

    [Fact]
    public async Task CreatePreferencesAsync_WithNegativeMinSalary_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.CreatePreferencesAsync(Guid.NewGuid(), -1000, 100000, "", ""));
    }

    [Fact]
    public async Task CreatePreferencesAsync_WithNegativeMaxSalary_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.CreatePreferencesAsync(Guid.NewGuid(), 50000, -100, "", ""));
    }

    [Fact]
    public async Task CreatePreferencesAsync_WithMinSalaryGreaterThanMax_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.CreatePreferencesAsync(Guid.NewGuid(), 100000, 50000, "", ""));
    }

    [Fact]
    public async Task UpdatePreferencesAsync_WithValidData_UpdatesPreferencesSuccessfully()
    {
        // Arrange
        var preferences = new Preferences
        {
            Id = 1,
            SalaryRange = new SalaryRange { Min = 50000m, Max = 100000m }
        };
        _preferencesRepositoryMock.GetById(1).Returns(preferences);

        // Act
        var result = await _preferencesService.UpdatePreferencesAsync(
            1, 60000m, 120000m, "FullTime,PartTime", "Remote");

        // Assert
        result.SalaryRange.Min.Should().Be(60000m);
        result.SalaryRange.Max.Should().Be(120000m);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_WithInvalidPreferenceId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.UpdatePreferencesAsync(0, 50000, 100000, "", ""));
    }

    [Fact]
    public async Task UpdatePreferencesAsync_WithInvalidSalaryRange_ThrowsArgumentException()
    {
        // Arrange
        var preferences = new Preferences { Id = 1 };
        _preferencesRepositoryMock.GetById(1).Returns(preferences);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.UpdatePreferencesAsync(1, 100000, 50000, "", ""));
    }

    [Fact]
    public async Task UpdatePreferencesAsync_WithNonExistentPreference_ThrowsInvalidOperationException()
    {
        // Arrange
        _preferencesRepositoryMock.GetById(999).Returns((Preferences?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _preferencesService.UpdatePreferencesAsync(999, 50000, 100000, "", ""));
    }

    [Fact]
    public async Task DeletePreferencesAsync_WithValidPreferenceId_DeletesSuccessfully()
    {
        // Arrange
        var preferences = new Preferences { Id = 1 };
        _preferencesRepositoryMock.GetById(1).Returns(preferences);

        // Act
        await _preferencesService.DeletePreferencesAsync(1);

        // Assert
        _preferencesRepositoryMock.Received(1).Remove(preferences);
    }

    [Fact]
    public async Task DeletePreferencesAsync_WithInvalidPreferenceId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.DeletePreferencesAsync(0));
    }

    [Fact]
    public async Task DeletePreferencesAsync_WithNonExistentPreference_ThrowsInvalidOperationException()
    {
        // Arrange
        _preferencesRepositoryMock.GetById(999).Returns((Preferences?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _preferencesService.DeletePreferencesAsync(999));
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WithValidUserId_ReturnsPreferences()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preferences = new Preferences
        {
            Id = 1,
            SupabaseUserId = userId,
            SalaryRange = new SalaryRange { Min = 50000m, Max = 100000m }
        };
        _preferencesRepositoryMock.Find(Arg.Any<System.Linq.Expressions.Expression<Func<Preferences, bool>>>())
            .Returns(new List<Preferences> { preferences });

        // Act
        var result = await _preferencesService.GetUserPreferencesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.SupabaseUserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _preferencesService.GetUserPreferencesAsync(Guid.Empty));
    }

    [Fact]
    public async Task GetUserPreferencesAsync_WithNoExistingPreferences_ReturnsNull()
    {
        // Arrange
        _preferencesRepositoryMock.Find(Arg.Any<System.Linq.Expressions.Expression<Func<Preferences, bool>>>())
            .Returns(new List<Preferences>());

        // Act
        var result = await _preferencesService.GetUserPreferencesAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}
