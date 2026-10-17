using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Named guards for repeat requests. Each throws a typed <see cref="RepeatException"/>.
/// </summary>
public static class RepeatGuard
{
    /// <summary>
    /// Throws when the mode is not a defined <see cref="RepeatMode"/>.
    /// </summary>
    public static void AgainstUndefinedMode(RepeatMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new RepeatValidationException($"Repeat mode '{(int)mode}' is not defined.");
        }
    }

    /// <summary>
    /// Throws when Rerun carries a source, or Recall/Replay lack one.
    /// </summary>
    public static void AgainstInvalidSource(RepeatMode mode, string? sourceExecutionId)
    {
        var hasSource = !string.IsNullOrWhiteSpace(sourceExecutionId);
        if (mode == RepeatMode.Rerun && hasSource)
        {
            throw new RepeatValidationException("Rerun must not specify a source execution.");
        }

        if (mode != RepeatMode.Rerun && !hasSource)
        {
            throw new RepeatValidationException($"{mode} requires a source execution.");
        }
    }

    /// <summary>
    /// Throws a not-found failure when the source was not located.
    /// </summary>
    public static void AgainstSourceNotFound(bool found, string sourceExecutionId)
    {
        if (!found)
        {
            throw new RepeatSourceNotFoundException($"Source execution '{sourceExecutionId}' was not found.");
        }
    }

    /// <summary>
    /// Throws a conflict failure when the source did not complete successfully.
    /// </summary>
    public static void AgainstSourceNotCompleted(bool completedSuccessfully, string sourceExecutionId)
    {
        if (!completedSuccessfully)
        {
            throw new RepeatConflictException($"Source execution '{sourceExecutionId}' has not completed successfully.");
        }
    }

    /// <summary>
    /// Throws a conflict failure when a Replay targets a source that cannot be replayed.
    /// </summary>
    public static void AgainstSourceNotReplayCapable(RepeatMode mode, bool replayCapable, string sourceExecutionId)
    {
        if (mode == RepeatMode.Replay && !replayCapable)
        {
            throw new RepeatConflictException($"Source execution '{sourceExecutionId}' is not replay capable.");
        }
    }

    /// <summary>
    /// Throws a forbidden failure when the caller may not repeat the source.
    /// </summary>
    public static void AgainstForbidden(bool allowed, string sourceExecutionId)
    {
        if (!allowed)
        {
            throw new RepeatForbiddenException($"Not permitted to repeat source execution '{sourceExecutionId}'.");
        }
    }
}
