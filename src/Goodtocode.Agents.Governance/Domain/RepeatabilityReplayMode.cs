namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Declares which of the three repeatability behaviors a governed execution is performing.
/// </summary>
public enum RepeatabilityReplayMode
{
    /// <summary>
    /// Fresh execution: re-collect evidence and/or re-invoke a non-deterministic model against
    /// the same prompt/rubric version, expecting the result to legitimately diverge from any
    /// prior execution. The default mode; a first-time execution is always a Rerun.
    /// </summary>
    Rerun = 0,

    /// <summary>
    /// Rehydrate a previously persisted governance record and its evidence without invoking
    /// inference or re-collecting. No drift is possible because nothing executes.
    /// </summary>
    Recall = 1,

    /// <summary>
    /// Resubmit the same collected evidence to the same model reference, model version, and
    /// prompt/rubric version to verify exact reproduction. Only supports an output-identity
    /// claim when the evaluator is genuinely deterministic.
    /// </summary>
    Replay = 2
}
