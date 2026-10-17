using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Default recorder writing <see cref="RepeatabilityRecord.ReplayMode"/>, <see cref="RepeatabilityRecord.SourceExecutionRef"/>,
/// and <see cref="RepeatabilityRecord.ActionsRun"/>.
/// </summary>
public sealed class DefaultRepeatRecorder : IRepeatRecorder
{
    /// <inheritdoc />
    public EvaluationGovernanceRecord Record(EvaluationGovernanceRecord record, RepeatRequest request, ActionType actionsRun)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(request);

        return record with
        {
            Repeatability = record.Repeatability with
            {
                ReplayMode = request.Mode,
                SourceExecutionRef = request.SourceExecutionId is null
                    ? null
                    : GovernanceReference.Parse(request.SourceExecutionId),
                ActionsRun = actionsRun
            }
        };
    }
}
