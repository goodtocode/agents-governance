using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Validation engine for governance records.
/// </summary>
public static class EvaluationGovernanceValidator
{
    /// <summary>
    /// Validates a governance record for required pillar data.
    /// </summary>
    /// <param name="governance">Governance record.</param>
    /// <returns>Validation result with all discovered issues.</returns>
    public static EvaluationGovernanceValidationResult Validate(EvaluationGovernanceRecord governance)
    {
        ArgumentNullException.ThrowIfNull(governance);

        var result = new EvaluationGovernanceValidationResult();

        if (string.IsNullOrWhiteSpace(governance.PolicyProfileVersion))
        {
            result.Add(nameof(EvaluationGovernanceRecord.PolicyProfileVersion), "PolicyProfileVersion is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Observability.TraceId))
        {
            result.Add(nameof(ObservabilityRecord.TraceId), "TraceId is required.");
        }

        if (governance.Observability.CorrelationId == Guid.Empty)
        {
            result.Add(nameof(ObservabilityRecord.CorrelationId), "CorrelationId is required.");
        }

        if (governance.Observability.EvidenceRefs.Count == 0)
        {
            result.Add(nameof(ObservabilityRecord.EvidenceRefs), "At least one EvidenceRef is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Repeatability.ModelRef))
        {
            result.Add(nameof(RepeatabilityRecord.ModelRef), "ModelRef is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Repeatability.ModelVersion))
        {
            result.Add(nameof(RepeatabilityRecord.ModelVersion), "ModelVersion is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Repeatability.PromptHash))
        {
            result.Add(nameof(RepeatabilityRecord.PromptHash), "PromptHash is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Repeatability.InputHash))
        {
            result.Add(nameof(RepeatabilityRecord.InputHash), "InputHash is required.");
        }

        if (governance.Repeatability.ReplayMode is RepeatabilityReplayMode.Recall or RepeatabilityReplayMode.Replay
            && governance.Repeatability.SourceExecutionRef is null)
        {
            result.Add(nameof(RepeatabilityRecord.SourceExecutionRef), "SourceExecutionRef is required for Recall and Replay.");
        }

        if (governance.Auditability.OwnerId == Guid.Empty)
        {
            result.Add(nameof(AuditabilityRecord.OwnerId), "OwnerId is required.");
        }

        if (governance.Auditability.TenantId == Guid.Empty)
        {
            result.Add(nameof(AuditabilityRecord.TenantId), "TenantId is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Auditability.PrincipalDisplay))
        {
            result.Add(nameof(AuditabilityRecord.PrincipalDisplay), "PrincipalDisplay is required.");
        }

        if (governance.Defensibility.PoliciesApplied.Count == 0)
        {
            result.Add(nameof(DefensibilityRecord.PoliciesApplied), "At least one policy reference is required.");
        }

        if (governance.Defensibility.JustificationRefs.Count == 0)
        {
            result.Add(nameof(DefensibilityRecord.JustificationRefs), "At least one justification reference is required.");
        }

        if (string.IsNullOrWhiteSpace(governance.Defensibility.ReasoningSummary))
        {
            result.Add(nameof(DefensibilityRecord.ReasoningSummary), "ReasoningSummary is required.");
        }

        if (governance.Defensibility.ConfidenceScore is < 0 or > 1)
        {
            result.Add(nameof(DefensibilityRecord.ConfidenceScore), "ConfidenceScore must be between 0 and 1.");
        }

        return result;
    }
}
