namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Default governance enforcer implementation.
/// </summary>
/// <remarks>
/// Repeatability hashes are never accepted as caller-supplied values. The enforcer always
/// derives <see cref="RepeatabilityRecord.PromptHash"/> and <see cref="RepeatabilityRecord.InputHash"/>
/// from the raw <see cref="GovernanceEvaluationRequest.RepeatabilityPromptContent"/> and
/// <see cref="GovernanceEvaluationRequest.RepeatabilityInputs"/> values, unconditionally, on every call
/// (including when those raw values are empty). This keeps hashing closed for modification while the
/// hashing algorithm itself remains open for extension via <see cref="IRepeatabilityHashStrategy"/>.
/// </remarks>
public sealed class GovernanceEnforcer(
    IEvaluationGovernancePromptComposer promptComposer,
    IRepeatabilityHashStrategy? hashStrategy = null) : IGovernanceEnforcer
{
    private readonly IEvaluationGovernancePromptComposer _promptComposer = promptComposer;
    private readonly IRepeatabilityHashStrategy _hashStrategy = hashStrategy ?? new DefaultRepeatabilityHashStrategy();

    /// <inheritdoc />
    public GovernedEvaluationResult Enforce(GovernanceEvaluationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var governance = ApplyRepeatabilityHashes(request);

        var validation = EvaluationGovernanceValidator.Validate(governance);
        if (!validation.IsValid)
        {
            throw new GovernanceValidationException(validation.Issues);
        }

        var promptContext = _promptComposer.Compose(new EvaluationGovernancePromptRequest
        {
            Governance = governance,
            Instruction = request.Instruction,
            IntentPolicy = request.IntentPolicy
        });

        return new GovernedEvaluationResult
        {
            Governance = governance,
            PromptContext = promptContext
        };
    }

    private EvaluationGovernanceRecord ApplyRepeatabilityHashes(GovernanceEvaluationRequest request)
    {
        var repeatability = request.Governance.Repeatability;
        var promptContent = request.RepeatabilityPromptContent ?? string.Empty;
        var intent = request.Governance.Intent;

        if (request.Instruction is { } instruction)
        {
            promptContent = $"{promptContent}\n[instruction:{instruction.ComputeHash()}]";
        }

        if (request.IntentPolicy is { } policy)
        {
            // Intent identity is derived here, never accepted from the caller, and folded into the prompt hash
            // so that a policy change surfaces as prompt drift in the replay guard.
            var policyHash = policy.ComputeLockHash();
            intent = intent with
            {
                PolicyId = policy.PolicyId,
                PolicyVersion = policy.Version,
                PolicyHash = policyHash
            };
            promptContent = $"{promptContent}\n[intent:{policy.PolicyId}@{policy.Version}#{policyHash}]";
        }

        var computedPromptHash = _hashStrategy.ComputePromptHash(promptContent);
        var computedInputHash = _hashStrategy.ComputeInputHash(request.RepeatabilityInputs);

        return request.Governance with
        {
            Intent = intent,
            Repeatability = repeatability with
            {
                PromptHash = computedPromptHash,
                InputHash = computedInputHash
            }
        };
    }
}
