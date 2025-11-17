using Bifrost.Contracts.ApplicationNotes;
using Bifrost.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bifrost.Api.Endpoints;

/// <summary>
/// Endpoints for managing application notes.
/// </summary>
public static class ApplicationNoteEndpoints
{
    public static void MapApplicationNoteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/applications/{applicationId}/notes")
            .WithTags("Application Notes")
            .WithOpenApi();

        group.MapPost("/", CreateNote)
            .WithName("CreateApplicationNote")
            .WithSummary("Create a new application note")
            .Produces<ApplicationNoteResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/{noteId}", GetNote)
            .WithName("GetApplicationNote")
            .WithSummary("Get an application note by ID")
            .Produces<ApplicationNoteResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapGet("/", GetApplicationNotes)
            .WithName("GetApplicationNotes")
            .WithSummary("Get all notes for an application")
            .Produces<List<ApplicationNoteResponse>>(StatusCodes.Status200OK);

        group.MapPut("/{noteId}", UpdateNote)
            .WithName("UpdateApplicationNote")
            .WithSummary("Update an application note")
            .Produces<ApplicationNoteResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);

        group.MapDelete("/{noteId}", DeleteNote)
            .WithName("DeleteApplicationNote")
            .WithSummary("Delete an application note")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateNote(
        long applicationId,
        CreateApplicationNoteRequest request,
        IApplicationNoteService noteService)
    {
        try
        {
            // TODO: Get userId from authenticated user context
            var userId = Guid.NewGuid();

            var note = await noteService.CreateNoteAsync(
                applicationId,
                userId,
                request.Note);

            var response = new ApplicationNoteResponse(
                note.Id,
                note.JobApplicationId,
                note.Note,
                note.SupabaseUserId,
                note.CreatedAt ?? DateTime.UtcNow,
                note.UpdatedAt);

            return Results.Created($"/api/applications/{applicationId}/notes/{note.Id}", response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "Application Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> GetNote(
        long applicationId,
        long noteId,
        IApplicationNoteService noteService)
    {
        try
        {
            var note = await noteService.GetNoteByIdAsync(noteId);
            if (note == null)
            {
                return Results.NotFound(new ProblemDetails
                {
                    Title = "Note Not Found",
                    Status = StatusCodes.Status404NotFound,
                    Detail = $"Note with ID {noteId} was not found."
                });
            }

            var response = new ApplicationNoteResponse(
                note.Id,
                note.JobApplicationId,
                note.Note,
                note.SupabaseUserId,
                note.CreatedAt ?? DateTime.UtcNow,
                note.UpdatedAt);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> GetApplicationNotes(
        long applicationId,
        IApplicationNoteService noteService)
    {
        try
        {
            var notes = await noteService.GetApplicationNotesAsync(applicationId);

            var responses = notes.Select(note => new ApplicationNoteResponse(
                note.Id,
                note.JobApplicationId,
                note.Note,
                note.SupabaseUserId,
                note.CreatedAt ?? DateTime.UtcNow,
                note.UpdatedAt)).ToList();

            return Results.Ok(responses);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> UpdateNote(
        long applicationId,
        long noteId,
        UpdateApplicationNoteRequest request,
        IApplicationNoteService noteService)
    {
        try
        {
            var note = await noteService.UpdateNoteAsync(noteId, request.Note);

            var response = new ApplicationNoteResponse(
                note.Id,
                note.JobApplicationId,
                note.Note,
                note.SupabaseUserId,
                note.CreatedAt ?? DateTime.UtcNow,
                note.UpdatedAt);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "Note Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }

    private static async Task<IResult> DeleteNote(
        long applicationId,
        long noteId,
        IApplicationNoteService noteService)
    {
        try
        {
            await noteService.DeleteNoteAsync(noteId);
            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Status = StatusCodes.Status400BadRequest,
                Detail = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Results.NotFound(new ProblemDetails
            {
                Title = "Note Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = ex.Message
            });
        }
    }
}
