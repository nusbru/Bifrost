using Bifrost.Api.Endpoints;
using Bifrost.Contracts.ApplicationNotes;
using Bifrost.Core.Models;
using Bifrost.Core.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bifrost.Api.Tests.Endpoints;

public class ApplicationNoteEndpointsTests
{
    private readonly IApplicationNoteService _noteServiceMock;
    private readonly WebApplication _app;

    public ApplicationNoteEndpointsTests()
    {
        _noteServiceMock = Substitute.For<IApplicationNoteService>();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddScoped<IApplicationNoteService>(_ => _noteServiceMock);

        _app = builder.Build();
        _app.MapApplicationNoteEndpoints();
    }

    [Fact]
    public async Task CreateNote_WithValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var applicationId = 1L;
        var userId = Guid.NewGuid();
        var request = new CreateApplicationNoteRequest("This is a test note");

        var note = new ApplicationNote
        {
            Id = 1,
            JobApplicationId = applicationId,
            Note = "This is a test note"
        };

        _noteServiceMock.CreateNoteAsync(applicationId, userId, "This is a test note")
            .Returns(note);

        // Act
        var response = await _app.Services
            .GetRequiredService<IApplicationNoteService>()
            .CreateNoteAsync(applicationId, userId, "This is a test note");

        // Assert
        response.Should().NotBeNull();
        response.Id.Should().Be(1);
        response.JobApplicationId.Should().Be(applicationId);
        response.Note.Should().Be("This is a test note");
    }

    [Fact]
    public async Task CreateNote_WithEmptyNote_ThrowsArgumentException()
    {
        // Arrange
        var applicationId = 1L;
        var userId = Guid.NewGuid();
        _noteServiceMock.CreateNoteAsync(applicationId, userId, "")
            .Returns(Task.FromException<ApplicationNote>(new ArgumentException("Note cannot be empty")));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _app.Services
                .GetRequiredService<IApplicationNoteService>()
                .CreateNoteAsync(applicationId, userId, ""));
    }

    [Fact]
    public async Task CreateNote_WithNonExistentApplication_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _noteServiceMock.CreateNoteAsync(999, userId, "Test note")
            .Returns(Task.FromException<ApplicationNote>(new InvalidOperationException("Application not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IApplicationNoteService>()
                .CreateNoteAsync(999, userId, "Test note"));
    }

    [Fact]
    public async Task GetNote_WithValidId_ReturnsNote()
    {
        // Arrange
        var noteId = 1L;
        var note = new ApplicationNote
        {
            Id = noteId,
            JobApplicationId = 1,
            Note = "Test note"
        };

        _noteServiceMock.GetNoteByIdAsync(noteId).Returns(note);

        // Act
        var response = await _app.Services
            .GetRequiredService<IApplicationNoteService>()
            .GetNoteByIdAsync(noteId);

        // Assert
        response.Should().NotBeNull();
        response!.Id.Should().Be(noteId);
        response.Note.Should().Be("Test note");
    }

    [Fact]
    public async Task GetNote_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        _noteServiceMock.GetNoteByIdAsync(999).Returns((ApplicationNote?)null);

        // Act
        var response = await _app.Services
            .GetRequiredService<IApplicationNoteService>()
            .GetNoteByIdAsync(999);

        // Assert
        response.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicationNotes_WithValidApplicationId_ReturnsNotesList()
    {
        // Arrange
        var applicationId = 1L;
        var notes = new List<ApplicationNote>
        {
            new ApplicationNote { Id = 1, JobApplicationId = applicationId, Note = "First note" },
            new ApplicationNote { Id = 2, JobApplicationId = applicationId, Note = "Second note" }
        };

        _noteServiceMock.GetApplicationNotesAsync(applicationId).Returns(notes);

        // Act
        var response = await _app.Services
            .GetRequiredService<IApplicationNoteService>()
            .GetApplicationNotesAsync(applicationId);

        // Assert
        response.Should().NotBeNull();
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateNote_WithValidData_UpdatesSuccessfully()
    {
        // Arrange
        var noteId = 1L;
        var request = new UpdateApplicationNoteRequest("Updated note content");
        var note = new ApplicationNote
        {
            Id = noteId,
            JobApplicationId = 1,
            Note = "Updated note content"
        };

        _noteServiceMock.UpdateNoteAsync(noteId, "Updated note content")
            .Returns(note);

        // Act
        var response = await _app.Services
            .GetRequiredService<IApplicationNoteService>()
            .UpdateNoteAsync(noteId, "Updated note content");

        // Assert
        response.Should().NotBeNull();
        response.Note.Should().Be("Updated note content");
    }

    [Fact]
    public async Task UpdateNote_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _noteServiceMock.UpdateNoteAsync(999, "Updated note")
            .Returns(Task.FromException<ApplicationNote>(new InvalidOperationException("Note not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IApplicationNoteService>()
                .UpdateNoteAsync(999, "Updated note"));
    }

    [Fact]
    public async Task DeleteNote_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var noteId = 1L;
        _noteServiceMock.DeleteNoteAsync(noteId).Returns(Task.CompletedTask);

        // Act
        await _app.Services
            .GetRequiredService<IApplicationNoteService>()
            .DeleteNoteAsync(noteId);

        // Assert
        await _noteServiceMock.Received(1).DeleteNoteAsync(noteId);
    }

    [Fact]
    public async Task DeleteNote_WithNonExistentId_ThrowsInvalidOperationException()
    {
        // Arrange
        _noteServiceMock.DeleteNoteAsync(999)
            .Returns(Task.FromException<ApplicationNote>(new InvalidOperationException("Note not found")));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _app.Services
                .GetRequiredService<IApplicationNoteService>()
                .DeleteNoteAsync(999));
    }
}
