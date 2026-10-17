using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Stamps repeat lineage onto a governance record.
/// </summary>
public interface IRepeatRecorder
{
    /// <summary>
    /// Returns a copy of the record carrying the mode, source reference, and actions that actually ran.
    /// </summary>
    EvaluationGovernanceRecord Record(EvaluationGovernanceRecord record, RepeatRequest request, ActionType actionsRun);
}
