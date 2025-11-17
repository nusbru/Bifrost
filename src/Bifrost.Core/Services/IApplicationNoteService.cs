using Bifrost.Core.Models;

namespace Bifrost.Core.Services;

/// <summary>
/// Service interface for managing ApplicationNote operations.
/// Handles CRUD operations and business logic for application notes.
/// </summary>
public interface IApplicationNoteService
{
    /// <summary>
    /// Creates a new application note.
    /// </summary>
    Task<ApplicationNote> CreateNoteAsync(long applicationId, Guid userId, string noteText);

    /// <summary>
    /// Updates an existing application note.
    /// </summary>
    Task<ApplicationNote> UpdateNoteAsync(long noteId, string noteText);

    /// <summary>
    /// Deletes an application note.
    /// </summary>
    Task DeleteNoteAsync(long noteId);

    /// <summary>
    /// Gets a note by ID.
    /// </summary>
    Task<ApplicationNote?> GetNoteByIdAsync(long noteId);

    /// <summary>
    /// Gets all notes for a job application.
    /// </summary>
    Task<IEnumerable<ApplicationNote>> GetApplicationNotesAsync(long applicationId);
}
