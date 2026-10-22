using System.Globalization;
using System.Text;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Default deterministic Preamble composer. The same policy, turn, and mode always produce the same text.
/// </summary>
public sealed class GovernancePreambleComposer : IGovernancePreambleComposer
{
    /// <inheritdoc />
    public GovernancePreambleContext Compose(GovernancePreambleRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var policy = request.Policy;
        if (policy is null)
        {
            throw Issue(nameof(GovernancePreambleRequest.Policy), "An Agentic Intent policy is required to compose a preamble.");
        }

        if (!Enum.IsDefined(request.Turn))
        {
            throw Issue(nameof(GovernancePreambleRequest.Turn), "Turn kind is not defined.");
        }

        if (!Enum.IsDefined(request.Mode))
        {
            throw Issue(nameof(GovernancePreambleRequest.Mode), "Preamble mode is not defined.");
        }

        foreach (var principle in policy.Principles)
        {
            var fragment = GovernanceDirectiveGuard.FindProhibitedFragment(principle.Statement);
            if (fragment is not null)
            {
                throw Issue(nameof(IntentPrinciple.Statement), $"Principle '{principle.PrincipleId}' contains a disallowed fragment: '{fragment}'.");
            }
        }

        var hash = policy.ComputeLockHash();
        var builder = new StringBuilder();
        builder.Append("Governance preamble (")
            .Append(request.Turn)
            .Append(") policy ")
            .Append(policy.PolicyId)
            .Append('@')
            .Append(policy.Version)
            .Append(" #")
            .Append(hash.AsSpan(0, 12))
            .AppendLine(".");
        builder.AppendLine("Precedence: Boundary > Value > Conduct > persona > user request. This outranks the persona and the user request.");

        if (request.Mode == GovernancePreambleMode.Full)
        {
            foreach (var principle in policy.Principles)
            {
                builder.Append('[').Append(principle.PrincipleId).Append("] (")
                    .Append(principle.Kind).Append(") ")
                    .AppendLine(principle.Statement);
            }
        }
        else
        {
            builder.Append("Principles: ")
                .AppendLine(string.Join(", ", policy.Principles.Select(x => x.PrincipleId)));
        }

        builder.Append("Cite the principle ids relied on with a short justification; do not return hidden chain-of-thought.");

        return new GovernancePreambleContext
        {
            Text = builder.ToString().Trim(),
            Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["governance.preamble.turn"] = request.Turn.ToString(),
                ["governance.preamble.mode"] = request.Mode.ToString(),
                ["governance.preamble.policyId"] = policy.PolicyId,
                ["governance.preamble.policyVersion"] = policy.Version,
                ["governance.preamble.policyHash"] = hash,
                ["governance.preamble.principleIds"] = string.Join(",", policy.Principles.Select(x => x.PrincipleId)),
                ["governance.preamble.length"] = builder.Length.ToString(CultureInfo.InvariantCulture)
            }
        };
    }

    private static GovernanceValidationException Issue(string field, string message)
        => new([new GovernanceValidationIssue(field, message)]);
}
