using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Raw governance input for enforcement.
/// </summary>
public sealed class GovernanceEvaluationRequest
{
    /// <summary>
    /// Gets or sets governance record.
    /// </summary>
    public EvaluationGovernanceRecord Governance { get; init; } = new();

    /// <summary>
    /// Gets or sets the optional Instruction (persona or caller text). Its hash is folded into the prompt hash,
    /// so a change is drift.
    /// </summary>
    public AgentInstruction? Instruction { get; init; }

    /// <summary>
    /// Gets or sets the Agentic Intent policy applied during enforcement. Its lock hash is stamped onto
    /// <see cref="EvaluationGovernanceRecord.Intent"/> and folded into the prompt hash, so a policy change is drift.
    /// </summary>
    public IntentPolicy? IntentPolicy { get; init; }

    /// <summary>
    /// Gets or sets optional raw prompt content used to deterministically compute
    /// <see cref="RepeatabilityRecord.PromptHash"/> during enforcement.
    /// </summary>
    public string? RepeatabilityPromptContent { get; init; }

    /// <summary>
    /// Gets or sets optional raw input values used to deterministically compute
    /// <see cref="RepeatabilityRecord.InputHash"/> during enforcement.
    /// </summary>
    public IReadOnlyDictionary<string, object?> RepeatabilityInputs { get; init; } =
        new Dictionary<string, object?>(StringComparer.Ordinal);
}

/// <summary>
/// Governance enforcement result.
/// </summary>
public sealed class GovernedEvaluationResult
{
    /// <summary>
    /// Gets or sets validated governance record.
    /// </summary>
    public EvaluationGovernanceRecord Governance { get; init; } = new();

    /// <summary>
    /// Gets or sets governance prompt context.
    /// </summary>
    public EvaluationGovernancePromptContext PromptContext { get; init; } = new();

    /// <summary>
    /// Gets the computed repeatability prompt hash.
    /// </summary>
    public string PromptHash => Governance.Repeatability.PromptHash;

    /// <summary>
    /// Gets the computed repeatability input hash.
    /// </summary>
    public string InputHash => Governance.Repeatability.InputHash;
}
