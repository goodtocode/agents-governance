using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Decides which actions a repeat runs. Replaceable.
/// </summary>
public interface IRepeatPolicy
{
    /// <summary>
    /// Produces the action plan for the request.
    /// </summary>
    RepeatPlan Plan(RepeatRequest request);
}
