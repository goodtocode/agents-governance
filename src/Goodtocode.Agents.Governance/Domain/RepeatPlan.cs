namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Immutable policy output stating which actions a repeat must run. Consumers run exactly those actions.
/// </summary>
/// <param name="Mode">The repeat mode the plan was produced for.</param>
/// <param name="Run">The set of actions that run.</param>
public readonly record struct RepeatPlan(RepeatMode Mode, ActionType Run)
{
    /// <summary>
    /// Returns whether the given action is part of this plan.
    /// </summary>
    /// <param name="action">The action to test.</param>
    public bool Runs(ActionType action) => (Run & action) == action;
}
