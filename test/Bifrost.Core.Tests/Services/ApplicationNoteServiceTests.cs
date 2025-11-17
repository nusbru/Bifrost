using Bifrost.Core.Models;
using Bifrost.Core.Repositories;
using Bifrost.Core.Services;
using FluentAssertions;
using NSubstitute;

namespace Bifrost.Core.Tests.Services;

public class ApplicationNoteServiceTests
{
    private readonly IApplicationNoteRepository _noteRepositoryMock;
    private readonly IJobApplicationRepository _applicationRepositoryMock;
    private readonly ApplicationNoteService _applicationNoteService;

    public ApplicationNoteServiceTests()
    {
        _noteRepositoryMock = Substitute.For<IApplicationNoteRepository>();
        _applicationRepositoryMock = Substitute.For<IJobApplicationRepository>();
        _applicationNoteService = new ApplicationNoteService(_noteRepositoryMock, _applicationRepositoryMock);
    }

    [Fact]
    public async Task CreateNoteAsync_WithValidData_CreatesNoteSuccessfully()
    {
        // Arrange
        var applicationId = 1L;
        var userId = Guid.NewGuid();
        var noteText = "Excellent interview";
        var application = new JobApplication { Id = applicationId };
        _applicationRepositoryMock.GetById(applicationId).Returns(application);

        // Act
        var result = await _applicationNoteService.CreateNoteAsync(applicationId, userId, noteText);

        // Assert
        result.Should().NotBeNull();
        result.JobApplicationId.Should().Be(applicationId);
        result.Note.Should().Be(noteText);
        result.SupabaseUserId.Should().Be(userId);

        await _noteRepositoryMock.Received(1).Add(Arg.Any<ApplicationNote>());
    }

    [Fact]
    public async Task CreateNoteAsync_WithInvalidApplicationId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _applicationNoteService.CreateNoteAsync(0, Guid.NewGuid(), "Note"));
    }

    [Fact]
    public async Task CreateNoteAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _applicationNoteService.CreateNoteAsync(1, Guid.Empty, "Note"));
    }

    [Fact]
    public async Task CreateNoteAsync_WithEmptyNoteText_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _applicationNoteService.CreateNoteAsync(1, Guid.NewGuid(), ""));
    }

    [Fact]
    public async Task CreateNoteAsync_WithNonExistentApplication_ThrowsInvalidOperationException()
    {
        // Arrange
        _applicationRepositoryMock.GetById(999).Returns((JobApplication?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _applicationNoteService.CreateNoteAsync(999, Guid.NewGuid(), "Note"));
    }

    [Fact]
    public async Task UpdateNoteAsync_WithValidData_UpdatesNoteSuccessfully()
    {
        // Arrange
        var note = new ApplicationNote { Id = 1, Note = "Old text" };
        _noteRepositoryMock.GetById(1).Returns(note);

        // Act
        var result = await _applicationNoteService.UpdateNoteAsync(1, "New text");

        // Assert
        result.Note.Should().Be("New text");
    }

    [Fact]
    public async Task UpdateNoteAsync_WithInvalidNoteId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _applicationNoteService.UpdateNoteAsync(0, "Text"));
    }

    [Fact]
    public async Task UpdateNoteAsync_WithEmptyNoteText_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _applicationNoteService.UpdateNoteAsync(1, ""));
    }

    [Fact]
    public async Task UpdateNoteAsync_WithNonExistentNote_ThrowsInvalidOperationException()
    {
        // Arrange
        _noteRepositoryMock.GetById(999).Returns((ApplicationNote?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _applicationNoteService.UpdateNoteAsync(999, "Text"));
    }

    [Fact]
    public async Task DeleteNoteAsync_WithValidNoteId_DeletesSuccessfully()
    {
        // Arrange
        var note = new ApplicationNote { Id = 1 };
        _noteRepositoryMock.GetById(1).Returns(note);

        // Act
        await _applicationNoteService.DeleteNoteAsync(1);

        // Assert
        _noteRepositoryMock.Received(1).Remove(note);
    }

    [Fact]
    public async Task DeleteNoteAsync_WithNonExistentNote_ThrowsInvalidOperationException()
    {
        // Arrange
        _noteRepositoryMock.GetById(999).Returns((ApplicationNote?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _applicationNoteService.DeleteNoteAsync(999));
    }

    [Fact]
    public async Task GetNoteByIdAsync_WithValidNoteId_ReturnsNote()
    {
        // Arrange
        var note = new ApplicationNote { Id = 1, Note = "Test" };
        _noteRepositoryMock.GetById(1).Returns(note);

        // Act
        var result = await _applicationNoteService.GetNoteByIdAsync(1);

        // Assert
        result.Should().BeEquivalentTo(note);
    }

    [Fact]
    public async Task GetNoteByIdAsync_WithNonExistentNote_ReturnsNull()
    {
        // Arrange
        _noteRepositoryMock.GetById(999).Returns((ApplicationNote?)null);

        // Act
        var result = await _applicationNoteService.GetNoteByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetApplicationNotesAsync_WithValidApplicationId_ReturnsNotes()
    {
        // Arrange
        var applicationId = 1L;
        var notes = new List<ApplicationNote>
        {
            new ApplicationNote { Id = 1, JobApplicationId = applicationId, Note = "Note 1" },
            new ApplicationNote { Id = 2, JobApplicationId = applicationId, Note = "Note 2" }
        };
        _noteRepositoryMock.Find(Arg.Any<System.Linq.Expressions.Expression<Func<ApplicationNote, bool>>>())
            .Returns(notes);

        // Act
        var result = await _applicationNoteService.GetApplicationNotesAsync(applicationId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(n => n.JobApplicationId.Should().Be(applicationId));
    }

    [Fact]
    public async Task GetApplicationNotesAsync_WithInvalidApplicationId_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _applicationNoteService.GetApplicationNotesAsync(0));
    }
}
