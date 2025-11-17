namespace Bifrost.Contracts.Common;

/// <summary>
/// Standard error response contract using ProblemDetails pattern.
/// Follows RFC 7807 - Problem Details for HTTP APIs.
/// </summary>
public record ErrorResponse(
    string Type,
    string Title,
    int Status,
    string Detail,
    string? Instance = null,
    Dictionary<string, object?>? Extensions = null);
