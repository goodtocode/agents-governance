using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Governance record carrying observability, auditability, repeatability, and defensibility details.
/// </summary>
public sealed record EvaluationGovernanceRecord
{
    /// <summary>
    /// Gets or sets policy profile version.
    /// </summary>
    public string PolicyProfileVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets observability data.
    /// </summary>
    public ObservabilityRecord Observability { get; init; } = new();

    /// <summary>
    /// Gets or sets repeatability data.
    /// </summary>
    public RepeatabilityRecord Repeatability { get; init; } = new();

    /// <summary>
    /// Gets or sets auditability data.
    /// </summary>
    public AuditabilityRecord Auditability { get; init; } = new();

    /// <summary>
    /// Gets or sets defensibility data.
    /// </summary>
    public DefensibilityRecord Defensibility { get; init; } = new();
}

/// <summary>
/// Observability dimensions for a governed execution.
/// </summary>
public sealed record ObservabilityRecord
{
    /// <summary>
    /// Gets or sets distributed trace identifier.
    /// </summary>
    public string TraceId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets cross-step correlation identifier.
    /// </summary>
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// Gets or sets evidence references.
    /// </summary>
    public IReadOnlyCollection<GovernanceReference> EvidenceRefs { get; init; } = [];
}

/// <summary>
/// Repeatability dimensions for a governed execution.
/// </summary>
public sealed record RepeatabilityRecord
{
    /// <summary>
    /// Gets or sets model reference identifier.
    /// </summary>
    public string ModelRef { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets model version.
    /// </summary>
    public string ModelVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets prompt hash.
    /// </summary>
    public string PromptHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets input hash.
    /// </summary>
    public string InputHash { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether deterministic replay is supported.
    /// </summary>
    public bool DeterministicReplaySupported { get; init; }

    /// <summary>
    /// Gets or sets optional execution seed.
    /// </summary>
    public int? Seed { get; init; }

    /// <summary>
    /// Gets or sets the declared repeatability replay mode. Defaults to <see cref="RepeatabilityReplayMode.Rerun"/>,
    /// since a first-time execution and a fresh re-collection are both Rerun behavior.
    /// </summary>
    public RepeatabilityReplayMode ReplayMode { get; init; } = RepeatabilityReplayMode.Rerun;

    /// <summary>
    /// Gets or sets the prior execution this record repeats. Required when <see cref="ReplayMode"/>
    /// is <see cref="RepeatabilityReplayMode.Recall"/> or <see cref="RepeatabilityReplayMode.Replay"/>.
    /// </summary>
    public GovernanceReference? SourceExecutionRef { get; init; }

    /// <summary>
    /// Gets or sets the hash of the governed structured output. Required in addition to
    /// <see cref="PromptHash"/> and <see cref="InputHash"/> before a Replay can claim
    /// <see cref="DeterministicReplaySupported"/>.
    /// </summary>
    public string OutputHash { get; init; } = string.Empty;
}

/// <summary>
/// Auditability dimensions for a governed execution.
/// </summary>
public sealed record AuditabilityRecord
{
    /// <summary>
    /// Gets or sets actor owner identifier.
    /// </summary>
    public Guid OwnerId { get; init; }

    /// <summary>
    /// Gets or sets actor tenant identifier.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Gets or sets principal display identifier.
    /// </summary>
    public string PrincipalDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets tool references involved in inference.
    /// </summary>
    public IReadOnlyCollection<GovernanceReference> ToolRefs { get; init; } = [];
}

/// <summary>
/// Defensibility dimensions for a governed execution.
/// </summary>
public sealed record DefensibilityRecord
{
    /// <summary>
    /// Gets or sets policy references applied during inference.
    /// </summary>
    public IReadOnlyCollection<GovernanceReference> PoliciesApplied { get; init; } = [];

    /// <summary>
    /// Gets or sets references backing justification claims.
    /// </summary>
    public IReadOnlyCollection<GovernanceReference> JustificationRefs { get; init; } = [];

    /// <summary>
    /// Gets or sets reasoning summary for decision explainability.
    /// </summary>
    public string ReasoningSummary { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets confidence score in range 0..1.
    /// </summary>
    public double? ConfidenceScore { get; init; }
}
