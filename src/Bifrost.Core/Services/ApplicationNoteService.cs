using Bifrost.Core.Models;
using Bifrost.Core.Repositories;

namespace Bifrost.Core.Services;

/// <summary>
/// Service for managing ApplicationNote operations.
/// </summary>
public class ApplicationNoteService : IApplicationNoteService
{
    private readonly IApplicationNoteRepository _noteRepository;
    private readonly IJobApplicationRepository _applicationRepository;

    public ApplicationNoteService(IApplicationNoteRepository noteRepository, IJobApplicationRepository applicationRepository)
    {
        _noteRepository = noteRepository ?? throw new ArgumentNullException(nameof(noteRepository));
        _applicationRepository = applicationRepository ?? throw new ArgumentNullException(nameof(applicationRepository));
    }

    public async Task<ApplicationNote> CreateNoteAsync(long applicationId, Guid userId, string noteText)
    {
        ValidateApplicationId(applicationId);
        ValidateUserId(userId);
        ValidateNoteText(noteText);

        var application = await _applicationRepository.GetById(applicationId)
            ?? throw new InvalidOperationException($"Job application with ID {applicationId} not found.");

        var note = new ApplicationNote
        {
            JobApplicationId = applicationId,
            Note = noteText.Trim(),
            SupabaseUserId = userId
        };

        await _noteRepository.Add(note);
        return note;
    }

    public async Task<ApplicationNote> UpdateNoteAsync(long noteId, string noteText)
    {
        ValidateNoteId(noteId);
        ValidateNoteText(noteText);

        var note = await _noteRepository.GetById(noteId)
            ?? throw new InvalidOperationException($"Application note with ID {noteId} not found.");

        note.Note = noteText.Trim();
        return note;
    }

    public async Task DeleteNoteAsync(long noteId)
    {
        ValidateNoteId(noteId);

        var note = await _noteRepository.GetById(noteId)
            ?? throw new InvalidOperationException($"Application note with ID {noteId} not found.");

        _noteRepository.Remove(note);
    }

    public async Task<ApplicationNote?> GetNoteByIdAsync(long noteId)
    {
        ValidateNoteId(noteId);
        return await _noteRepository.GetById(noteId);
    }

    public async Task<IEnumerable<ApplicationNote>> GetApplicationNotesAsync(long applicationId)
    {
        ValidateApplicationId(applicationId);
        return await _noteRepository.Find(n => n.JobApplicationId == applicationId);
    }

    private static void ValidateApplicationId(long applicationId)
    {
        if (applicationId <= 0)
            throw new ArgumentException("Application ID must be greater than zero.", nameof(applicationId));
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));
    }

    private static void ValidateNoteText(string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
            throw new ArgumentException("Note text cannot be empty.", nameof(noteText));
    }

    private static void ValidateNoteId(long noteId)
    {
        if (noteId <= 0)
            throw new ArgumentException("Note ID must be greater than zero.", nameof(noteId));
    }
}
