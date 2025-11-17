namespace Bifrost.Contracts.ApplicationNotes;

/// <summary>
/// Request contract for creating an application note.
/// </summary>
public record CreateApplicationNoteRequest(
    Guid UserId,
    string Note);
