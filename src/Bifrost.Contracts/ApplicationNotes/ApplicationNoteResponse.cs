namespace Bifrost.Contracts.ApplicationNotes;

/// <summary>
/// Response contract for application note information.
/// </summary>
public record ApplicationNoteResponse(
    long Id,
    long JobApplicationId,
    string Note,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
