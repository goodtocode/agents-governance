namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Shared guard that rejects text attempting to switch off or override governance. Applied to personas,
/// extension directives, policy statements, and preambles.
/// </summary>
public static class GovernanceDirectiveGuard
{
    private static readonly string[] ProhibitedFragments =
    [
        "ignore previous instruction",
        "ignore prior instruction",
        "disregard previous instruction",
        "disable governance",
        "skip governance",
        "override governance"
    ];

    /// <summary>
    /// Returns the first prohibited fragment found in the text, or null when the text is clean.
    /// </summary>
    public static string? FindProhibitedFragment(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        return ProhibitedFragments.FirstOrDefault(x => text.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
}
