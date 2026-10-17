using Goodtocode.Agents.Governance.Application;

namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Immutable request to repeat a prior governed execution. Rerun forbids a source; Recall and Replay require one.
/// </summary>
public sealed record RepeatRequest
{
    private RepeatRequest(RepeatMode mode, string? sourceExecutionId)
    {
        Mode = mode;
        SourceExecutionId = sourceExecutionId;
    }

    /// <summary>
    /// Gets the requested repeat mode.
    /// </summary>
    public RepeatMode Mode { get; }

    /// <summary>
    /// Gets the identifier of the execution being repeated, or null for Rerun.
    /// </summary>
    public string? SourceExecutionId { get; }

    /// <summary>
    /// Creates a validated request.
    /// </summary>
    /// <exception cref="RepeatValidationException">The mode and source combination is invalid.</exception>
    public static RepeatRequest Create(RepeatMode mode, string? sourceExecutionId = null)
    {
        RepeatGuard.AgainstUndefinedMode(mode);
        var source = string.IsNullOrWhiteSpace(sourceExecutionId) ? null : sourceExecutionId.Trim();
        RepeatGuard.AgainstInvalidSource(mode, source);
        return new RepeatRequest(mode, source);
    }

    /// <summary>
    /// Creates a validated request from a Guid source.
    /// </summary>
    public static RepeatRequest Create(RepeatMode mode, Guid? sourceExecutionId) =>
        Create(mode, sourceExecutionId?.ToString("D"));

    /// <summary>
    /// Gets a fresh-execution request.
    /// </summary>
    public static RepeatRequest Rerun() => new(RepeatMode.Rerun, null);
}
