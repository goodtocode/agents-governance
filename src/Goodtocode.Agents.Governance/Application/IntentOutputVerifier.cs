using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Pluggable check, such as a classifier or rule engine, that inspects an output against the governing policy.
/// </summary>
public interface IIntentOutputRule
{
    /// <summary>
    /// Returns the violations found, or an empty list when the output passes.
    /// </summary>
    Task<IReadOnlyList<IntentViolation>> CheckAsync(IntentPolicy policy, IntentOutput output, CancellationToken cancellationToken = default);
}

/// <summary>
/// Outcome of verifying an output against Agentic Intent.
/// </summary>
/// <param name="PolicyId">The policy verified against.</param>
/// <param name="PolicyVersion">The policy version.</param>
/// <param name="PolicyHash">The policy lock hash.</param>
/// <param name="Violations">The failures; empty when the output passed.</param>
public sealed record IntentVerificationResult(
    string PolicyId,
    string PolicyVersion,
    string PolicyHash,
    IReadOnlyList<IntentViolation> Violations)
{
    /// <summary>
    /// Gets a value indicating whether the output passed.
    /// </summary>
    public bool Passed => Violations.Count == 0;

    /// <summary>
    /// Throws when the output did not pass.
    /// </summary>
    /// <exception cref="GovernanceValidationException">The output has violations.</exception>
    public void EnsurePassed()
    {
        if (!Passed)
        {
            throw new GovernanceValidationException(
                [.. Violations.Select(x => new GovernanceValidationIssue(x.PrincipleId ?? x.Code.ToString(), x.Message))]);
        }
    }
}

/// <summary>
/// Verifies governed outputs against Agentic Intent. Prompting is guidance; verification is the enforcement step.
/// </summary>
public interface IIntentOutputVerifier
{
    /// <summary>
    /// Verifies an output against a policy.
    /// </summary>
    Task<IntentVerificationResult> VerifyAsync(IntentPolicy policy, IntentOutput output, CancellationToken cancellationToken = default);
}

/// <summary>
/// Options for <see cref="IntentOutputVerifier"/>.
/// </summary>
public sealed class IntentOutputVerifierOptions
{
    /// <summary>
    /// Gets or sets whether at least one citation is required. Defaults to true.
    /// </summary>
    public bool CitationsRequired { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum justification length. Longer text is treated as a reasoning trace. Defaults to 500.
    /// </summary>
    public int MaxJustificationLength { get; set; } = 500;
}

/// <summary>
/// Default verifier: requires citations of known principle ids with short justifications, then runs the
/// pluggable rules. Violations are reported, never thrown, so the host decides to reject or flag.
/// </summary>
public sealed class IntentOutputVerifier : IIntentOutputVerifier
{
    private readonly IReadOnlyList<IIntentOutputRule> _rules;
    private readonly IntentOutputVerifierOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="IntentOutputVerifier"/> class.
    /// </summary>
    public IntentOutputVerifier(IEnumerable<IIntentOutputRule>? rules = null, IntentOutputVerifierOptions? options = null)
    {
        _rules = [.. rules ?? []];
        _options = options ?? new IntentOutputVerifierOptions();
    }

    /// <inheritdoc />
    public async Task<IntentVerificationResult> VerifyAsync(IntentPolicy policy, IntentOutput output, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(output);

        var violations = new List<IntentViolation>();
        var known = policy.Principles.Select(x => x.PrincipleId).ToHashSet(StringComparer.Ordinal);

        if (_options.CitationsRequired && output.Citations.Count == 0)
        {
            violations.Add(new IntentViolation(IntentViolationCode.MissingCitation, null, "The output cites no Agentic Intent principle."));
        }

        foreach (var citation in output.Citations)
        {
            if (!known.Contains(citation.PrincipleId))
            {
                violations.Add(new IntentViolation(IntentViolationCode.UnknownPrinciple, citation.PrincipleId, $"Principle '{citation.PrincipleId}' is not in policy '{policy.PolicyId}'."));
            }

            if (string.IsNullOrWhiteSpace(citation.Justification))
            {
                violations.Add(new IntentViolation(IntentViolationCode.MissingJustification, citation.PrincipleId, $"Citation of '{citation.PrincipleId}' has no justification."));
            }
            else if (citation.Justification.Length > _options.MaxJustificationLength)
            {
                violations.Add(new IntentViolation(IntentViolationCode.JustificationTooLong, citation.PrincipleId, $"Justification for '{citation.PrincipleId}' exceeds {_options.MaxJustificationLength} characters; provide a summary, not the reasoning trace."));
            }
        }

        foreach (var rule in _rules)
        {
            violations.AddRange(await rule.CheckAsync(policy, output, cancellationToken).ConfigureAwait(false));
        }

        return new IntentVerificationResult(policy.PolicyId, policy.Version, policy.ComputeLockHash(), violations);
    }
}
