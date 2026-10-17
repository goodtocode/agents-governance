using System.ComponentModel.DataAnnotations;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Options for the default repeat policy.
/// </summary>
public sealed class RepeatabilityOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether Recall also runs the record action. Defaults to false (pure read).
    /// </summary>
    [Required]
    public bool? RecallRecords { get; set; } = false;
}
