using Goodtocode.Agents.Governance.Domain;
using Microsoft.Extensions.Options;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Table-driven default policy: Rerun runs everything; Replay skips collect; Recall is a pure read unless
/// <see cref="RepeatabilityOptions.RecallRecords"/> is enabled.
/// </summary>
public sealed class DefaultRepeatPolicy : IRepeatPolicy
{
    private readonly bool _recallRecords;

    /// <summary>
    /// Initializes a policy with default options.
    /// </summary>
    public DefaultRepeatPolicy()
        : this(new RepeatabilityOptions())
    {
    }

    /// <summary>
    /// Initializes a policy with explicit options.
    /// </summary>
    public DefaultRepeatPolicy(RepeatabilityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _recallRecords = options.RecallRecords ?? false;
    }

    /// <summary>
    /// Initializes a policy from DI options.
    /// </summary>
    public DefaultRepeatPolicy(IOptions<RepeatabilityOptions> options)
        : this(options.Value)
    {
    }

    /// <inheritdoc />
    public RepeatPlan Plan(RepeatRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return request.Mode switch
        {
            RepeatMode.Rerun => new RepeatPlan(RepeatMode.Rerun, ActionType.All),
            RepeatMode.Replay => new RepeatPlan(RepeatMode.Replay, ActionType.Evaluate | ActionType.Record),
            RepeatMode.Recall => new RepeatPlan(RepeatMode.Recall, _recallRecords ? ActionType.Record : ActionType.None),
            _ => throw new RepeatValidationException($"Repeat mode '{(int)request.Mode}' is not defined.")
        };
    }
}
