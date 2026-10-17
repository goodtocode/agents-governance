namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Standard failure categories for repeat requests, so apps can map them to 400, 403, 404, and 409.
/// </summary>
public enum RepeatFailureCategory
{
    /// <summary>The request is malformed (map to 400).</summary>
    Validation = 0,

    /// <summary>The source execution does not exist in scope (map to 404).</summary>
    NotFound = 1,

    /// <summary>The source is not in a state that allows the repeat (map to 409).</summary>
    Conflict = 2,

    /// <summary>The caller may not repeat the source (map to 403).</summary>
    Forbidden = 3
}

/// <summary>
/// Base type for repeat failures.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RepeatException"/> class.
/// </remarks>
public abstract class RepeatException(RepeatFailureCategory category, string message) : Exception(message)
{

    /// <summary>
    /// Gets the failure category.
    /// </summary>
    public RepeatFailureCategory Category { get; } = category;
}

/// <summary>
/// The repeat request is malformed.
/// </summary>
public sealed class RepeatValidationException(string message) : RepeatException(RepeatFailureCategory.Validation, message);

/// <summary>
/// The source execution was not found within the caller's scope.
/// </summary>
public sealed class RepeatSourceNotFoundException(string message) : RepeatException(RepeatFailureCategory.NotFound, message);

/// <summary>
/// The source execution cannot be repeated in this mode (for example not completed or not replay capable).
/// </summary>
public sealed class RepeatConflictException(string message) : RepeatException(RepeatFailureCategory.Conflict, message);

/// <summary>
/// The caller is not allowed to repeat the source execution.
/// </summary>
public sealed class RepeatForbiddenException(string message) : RepeatException(RepeatFailureCategory.Forbidden, message);
