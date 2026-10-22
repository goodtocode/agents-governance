namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// A decision's reference to an Agentic Intent principle it relied on, with a short justification.
/// The justification is a summary, never the reasoning trace.
/// </summary>
/// <param name="PrincipleId">The id of the cited principle.</param>
/// <param name="Justification">A short statement of how the principle applied.</param>
public sealed record IntentCitation(string PrincipleId, string Justification);

/// <summary>
/// A governed model output presented for Agentic Intent verification.
/// </summary>
/// <param name="Text">The output text.</param>
/// <param name="Citations">The principle citations the output declares.</param>
public sealed record IntentOutput(string Text, IReadOnlyList<IntentCitation> Citations);

/// <summary>
/// Why an output failed Agentic Intent verification.
/// </summary>
public enum IntentViolationCode
{
    /// <summary>
    /// Citations are required and none were provided.
    /// </summary>
    MissingCitation = 1,

    /// <summary>
    /// A citation names a principle that is not in the governing policy.
    /// </summary>
    UnknownPrinciple = 2,

    /// <summary>
    /// A citation has no justification.
    /// </summary>
    MissingJustification = 3,

    /// <summary>
    /// A justification is longer than allowed, which suggests a reasoning trace rather than a summary.
    /// </summary>
    JustificationTooLong = 4,

    /// <summary>
    /// A pluggable rule rejected the output.
    /// </summary>
    RuleViolation = 5
}

/// <summary>
/// One verification failure.
/// </summary>
/// <param name="Code">The failure kind.</param>
/// <param name="PrincipleId">The principle concerned, when one applies.</param>
/// <param name="Message">A human-readable explanation.</param>
public sealed record IntentViolation(IntentViolationCode Code, string? PrincipleId, string Message);
