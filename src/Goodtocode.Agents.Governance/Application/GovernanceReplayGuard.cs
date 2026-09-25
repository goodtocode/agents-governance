namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Snapshot used to compare repeatability constraints between executions.
/// </summary>
public sealed record GovernanceReplaySnapshot(
    string PolicyProfileVersion,
    string ModelRef,
    string ModelVersion,
    string PromptHash,
    string InputHash,
    string OutputHash = "");

/// <summary>
/// Describes the drift observed between a baseline and a rerun snapshot. A Rerun is allowed to
/// diverge; this result is informational for defensibility, not a validation failure.
/// </summary>
public sealed record GovernanceRerunComparison(
    bool ModelReferenceChanged,
    bool ModelVersionChanged,
    bool PromptChanged,
    bool InputChanged,
    bool OutputChanged)
{
    /// <summary>
    /// Gets a value indicating whether the rerun reproduced the baseline exactly.
    /// </summary>
    public bool IsIdenticalToBaseline =>
        !ModelReferenceChanged && !ModelVersionChanged && !PromptChanged && !InputChanged && !OutputChanged;
}

/// <summary>
/// Guards against replay drift by enforcing exact snapshot matches.
/// </summary>
public static class GovernanceReplayGuard
{
    /// <summary>
    /// Validates that the current snapshot matches the baseline snapshot exactly. Use for the
    /// <c>Replay</c> mode, where reproduction is expected to be exact.
    /// </summary>
    /// <param name="baseline">Baseline snapshot from persisted execution.</param>
    /// <param name="current">Current snapshot from replay attempt.</param>
    public static void EnsureExactReplay(GovernanceReplaySnapshot baseline, GovernanceReplaySnapshot current)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(current);

        if (!string.Equals(baseline.PolicyProfileVersion, current.PolicyProfileVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Replay drift detected: policy profile version mismatch.");
        }

        if (!string.Equals(baseline.ModelRef, current.ModelRef, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Replay drift detected: model reference mismatch.");
        }

        if (!string.Equals(baseline.ModelVersion, current.ModelVersion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Replay drift detected: model version mismatch.");
        }

        if (!string.Equals(baseline.PromptHash, current.PromptHash, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Replay drift detected: prompt hash mismatch.");
        }

        if (!string.Equals(baseline.InputHash, current.InputHash, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Replay drift detected: input hash mismatch.");
        }

        if (baseline.OutputHash.Length > 0 && current.OutputHash.Length > 0
            && !string.Equals(baseline.OutputHash, current.OutputHash, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Replay drift detected: output hash mismatch.");
        }
    }

    /// <summary>
    /// Compares a rerun snapshot against its baseline without throwing. Use for the
    /// <c>Rerun</c> mode, where the collected evidence and/or model output are allowed to
    /// legitimately diverge from the baseline; the result is a defensibility signal, not a
    /// validation failure.
    /// </summary>
    /// <param name="baseline">Baseline snapshot from the prior execution.</param>
    /// <param name="current">Snapshot from the rerun.</param>
    public static GovernanceRerunComparison CompareRerun(GovernanceReplaySnapshot baseline, GovernanceReplaySnapshot current)
    {
        ArgumentNullException.ThrowIfNull(baseline);
        ArgumentNullException.ThrowIfNull(current);

        return new GovernanceRerunComparison(
            ModelReferenceChanged: !string.Equals(baseline.ModelRef, current.ModelRef, StringComparison.Ordinal),
            ModelVersionChanged: !string.Equals(baseline.ModelVersion, current.ModelVersion, StringComparison.Ordinal),
            PromptChanged: !string.Equals(baseline.PromptHash, current.PromptHash, StringComparison.Ordinal),
            InputChanged: !string.Equals(baseline.InputHash, current.InputHash, StringComparison.Ordinal),
            OutputChanged: !string.Equals(baseline.OutputHash, current.OutputHash, StringComparison.Ordinal));
    }
}
