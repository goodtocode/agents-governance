using System.Globalization;
using System.Text;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Default deterministic governance prompt composer.
/// </summary>
public sealed class EvaluationGovernancePromptComposer : IEvaluationGovernancePromptComposer
{
    private static readonly string[] CoreDirectives =
    [
        "Governance assurance pillars: Observability, Repeatability, Auditability, Defensibility.",
        "Do not repeat prior inference if replay data is available; reuse persisted governed outputs.",
        "When inference is required, treat prior persisted values as constraints and starting context.",
        "Do not introduce novel interpretations that drift from persisted policy and evidence.",
        "Return concise reasoning summaries only; do not return hidden chain-of-thought.",
        "Every score and decision must include explicit evidence and policy justification.",
        "Always include confidence score and uncertainty when evidence is incomplete."
    ];

    private readonly IGovernanceDirectiveExtension[] _extensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationGovernancePromptComposer"/> class.
    /// </summary>
    public EvaluationGovernancePromptComposer()
        : this([])
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EvaluationGovernancePromptComposer"/> class.
    /// </summary>
    /// <param name="extensions">Optional governance directive extensions.</param>
    public EvaluationGovernancePromptComposer(IEnumerable<IGovernanceDirectiveExtension>? extensions)
    {
        _extensions = (extensions ?? [])
            .OrderBy(x => x.Order)
            .ThenBy(x => x.ExtensionId, StringComparer.Ordinal)
            .ToArray();

        GuardAgainstDuplicateExtensionIds(_extensions);
    }

    /// <inheritdoc />
    public EvaluationGovernancePromptContext Compose(EvaluationGovernancePromptRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validation = EvaluationGovernanceValidator.Validate(request.Governance);
        if (!validation.IsValid)
        {
            throw new GovernanceValidationException(validation.Issues);
        }

        var builder = new StringBuilder();
        var metadata = BuildCoreMetadata(request.Governance);

        if (request.Governance.Intent.Required && request.IntentPolicy is null)
        {
            throw new GovernanceValidationException(
            [
                new GovernanceValidationIssue(nameof(EvaluationGovernancePromptRequest.IntentPolicy), "An intent policy is required by the governance record.")
            ]);
        }

        if (request.IntentPolicy is not null
            && !string.IsNullOrEmpty(request.Governance.Intent.PolicyHash)
            && !string.Equals(request.Governance.Intent.PolicyHash, request.IntentPolicy.ComputeLockHash(), StringComparison.Ordinal))
        {
            throw new GovernanceValidationException(
            [
                new GovernanceValidationIssue(nameof(IntentRecord.PolicyHash), "Intent policy hash does not match the supplied policy.")
            ]);
        }

        if (request.Instruction is { } instruction)
        {
            // Re-validated because the record can be constructed with a non-validated value via `with`.
            var checkedInstruction = AgentInstruction.Create(instruction.Text, instruction.Scope);
            builder.AppendLine(checkedInstruction.Text);
            builder.AppendLine();
        }

        foreach (var coreDirective in CoreDirectives)
        {
            builder.AppendLine(coreDirective);
        }

        builder.Append("Policy profile version: ");
        builder.Append(request.Governance.PolicyProfileVersion);
        builder.AppendLine(".");

        if (request.IntentPolicy is { } policy)
        {
            builder.AppendLine(IntentInstruction.Render(policy));

            metadata["governance.intent.policyId"] = policy.PolicyId;
            metadata["governance.intent.policyVersion"] = policy.Version;
            metadata["governance.intent.policyHash"] = policy.ComputeLockHash();
            metadata["governance.intent.principleIds"] = string.Join(",", policy.Principles.Select(p => p.PrincipleId));
        }

        ApplyExtensions(builder, metadata, request);

        return new EvaluationGovernancePromptContext
        {
            SystemInstruction = builder.ToString().Trim(),
            Metadata = metadata
        };
    }

    private static Dictionary<string, string> BuildCoreMetadata(EvaluationGovernanceRecord governance)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["governance.policyProfileVersion"] = governance.PolicyProfileVersion,
            ["governance.observability.traceId"] = governance.Observability.TraceId,
            ["governance.observability.correlationId"] = governance.Observability.CorrelationId.ToString(),
            ["governance.repeatability.modelRef"] = governance.Repeatability.ModelRef,
            ["governance.repeatability.modelVersion"] = governance.Repeatability.ModelVersion,
            ["governance.repeatability.promptHash"] = governance.Repeatability.PromptHash,
            ["governance.repeatability.inputHash"] = governance.Repeatability.InputHash,
            ["governance.auditability.ownerId"] = governance.Auditability.OwnerId.ToString(),
            ["governance.auditability.tenantId"] = governance.Auditability.TenantId.ToString(),
            ["governance.auditability.principalDisplay"] = governance.Auditability.PrincipalDisplay,
            ["governance.defensibility.confidenceScore"] = governance.Defensibility.ConfidenceScore?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
        };
    }

    private static void GuardAgainstDuplicateExtensionIds(IGovernanceDirectiveExtension[] extensions)
    {
        var duplicates = extensions
            .GroupBy(x => x.ExtensionId, StringComparer.Ordinal)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();

        if (duplicates.Length == 0)
        {
            return;
        }

        throw new GovernanceValidationException(
        [
            new GovernanceValidationIssue(
                nameof(IGovernanceDirectiveExtension.ExtensionId),
                $"Duplicate governance extension ids are not allowed: {string.Join(", ", duplicates)}")
        ]);
    }

    private void ApplyExtensions(
        StringBuilder builder,
        Dictionary<string, string> metadata,
        EvaluationGovernancePromptRequest request)
    {
        if (_extensions.Length == 0)
        {
            metadata["governance.extensions.applied"] = string.Empty;
            return;
        }

        var appliedExtensions = new List<string>();

        foreach (var extension in _extensions)
        {
            var contribution = extension.Build(request);
            if (contribution is null)
            {
                throw new GovernanceValidationException(
                [
                    new GovernanceValidationIssue(
                        nameof(IGovernanceDirectiveExtension.Build),
                        $"Extension '{extension.ExtensionId}' returned null contribution.")
                ]);
            }

            var extensionKey = NormalizeMetadataSegment(extension.ExtensionId);
            if (string.IsNullOrWhiteSpace(extensionKey))
            {
                throw new GovernanceValidationException(
                [
                    new GovernanceValidationIssue(
                        nameof(IGovernanceDirectiveExtension.ExtensionId),
                        "ExtensionId must contain at least one letter or number.")
                ]);
            }

            foreach (var directive in contribution.Directives)
            {
                GuardAgainstInvalidExtensionDirective(extension.ExtensionId, directive);
                builder.AppendLine(directive.Trim());
            }

            var extensionPrefix = $"governance.extensions.{extensionKey}";
            metadata[$"{extensionPrefix}.id"] = extension.ExtensionId;
            metadata[$"{extensionPrefix}.version"] = extension.ExtensionVersion;
            metadata[$"{extensionPrefix}.order"] = extension.Order.ToString(CultureInfo.InvariantCulture);

            foreach (var pair in contribution.Metadata)
            {
                GuardAgainstInvalidExtensionMetadata(extension.ExtensionId, pair);
                var metadataSegment = NormalizeMetadataSegment(pair.Key);
                if (string.IsNullOrWhiteSpace(metadataSegment))
                {
                    throw new GovernanceValidationException(
                    [
                        new GovernanceValidationIssue(
                            nameof(GovernanceDirectiveContribution.Metadata),
                            $"Extension '{extension.ExtensionId}' emitted metadata key '{pair.Key}' that cannot be normalized.")
                    ]);
                }

                var metadataKey = $"{extensionPrefix}.meta.{metadataSegment}";

                if (metadata.ContainsKey(metadataKey))
                {
                    throw new GovernanceValidationException(
                    [
                        new GovernanceValidationIssue(
                            nameof(GovernanceDirectiveContribution.Metadata),
                            $"Duplicate extension metadata key '{metadataKey}' is not allowed.")
                    ]);
                }

                metadata[metadataKey] = pair.Value;
            }

            appliedExtensions.Add($"{extension.ExtensionId}@{extension.ExtensionVersion}");
        }

        metadata["governance.extensions.applied"] = string.Join(",", appliedExtensions);
    }

    private static void GuardAgainstInvalidExtensionDirective(string extensionId, string directive)
    {
        if (string.IsNullOrWhiteSpace(directive))
        {
            throw new GovernanceValidationException(
            [
                new GovernanceValidationIssue(
                    nameof(GovernanceDirectiveContribution.Directives),
                    $"Extension '{extensionId}' emitted an empty directive.")
            ]);
        }

        foreach (var prohibitedFragment in new[] { GovernanceDirectiveGuard.FindProhibitedFragment(directive) })
        {
            if (prohibitedFragment is not null)
            {
                throw new GovernanceValidationException(
                [
                    new GovernanceValidationIssue(
                        nameof(GovernanceDirectiveContribution.Directives),
                        $"Extension '{extensionId}' emitted a disallowed directive fragment: '{prohibitedFragment}'.")
                ]);
            }
        }
    }

    private static void GuardAgainstInvalidExtensionMetadata(string extensionId, KeyValuePair<string, string> pair)
    {
        if (string.IsNullOrWhiteSpace(pair.Key))
        {
            throw new GovernanceValidationException(
            [
                new GovernanceValidationIssue(
                    nameof(GovernanceDirectiveContribution.Metadata),
                    $"Extension '{extensionId}' emitted an empty metadata key.")
            ]);
        }
    }

    private static string NormalizeMetadataSegment(string value)
    {
        var chars = value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant)
            .ToArray();

        return new string(chars);
    }
}
