namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Exception raised when a governance record fails validation.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="GovernanceValidationException"/> class.
/// </remarks>
/// <param name="issues">Validation issues.</param>
public sealed class GovernanceValidationException(IReadOnlyCollection<GovernanceValidationIssue> issues) : Exception(BuildMessage(issues))
{

    /// <summary>
    /// Gets validation issues.
    /// </summary>
    public IReadOnlyCollection<GovernanceValidationIssue> Issues { get; } = issues;

    private static string BuildMessage(IReadOnlyCollection<GovernanceValidationIssue> issues)
    {
        if (issues.Count == 0)
        {
            return "Governance validation failed.";
        }

        return $"Governance validation failed: {string.Join("; ", issues.Select(x => $"{x.Field}: {x.Message}"))}";
    }
}
