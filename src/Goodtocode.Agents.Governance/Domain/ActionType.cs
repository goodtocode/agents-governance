namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// The kind of action a workflow step performs. Governance classifies every step as one of three
/// operations so repeat policy and audit records can reason about it. Also used as a set of actions
/// (for example, the actions a repeat will run), hence <see cref="FlagsAttribute"/>.
/// </summary>
[Flags]
public enum ActionType
{
    /// <summary>
    /// No action.
    /// </summary>
    None = 0,

    /// <summary>
    /// Brings new facts in from outside the run (for example a query or tool call). Produces evidence and is not repeatable by itself.
    /// </summary>
    Collect = 1,

    /// <summary>
    /// Derives a judgment from existing evidence (for example a rubric or an LLM prompt). Produces a finding.
    /// </summary>
    Evaluate = 2,

    /// <summary>
    /// Projects the result into a durable or presentable form (for example a template or an LLM prompt).
    /// </summary>
    Record = 4,

    /// <summary>
    /// Collect, Evaluate, and Record.
    /// </summary>
    All = Collect | Evaluate | Record
}
