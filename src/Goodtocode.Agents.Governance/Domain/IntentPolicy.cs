using System.Security.Cryptography;
using System.Text;

namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Agentic Intent: an immutable, versioned set of boundaries, values, and conduct principles that governs
/// what an agent is trying to do. It is the first governance policy; the four pillars make it observable,
/// repeatable, auditable, and defensible.
/// </summary>
/// <remarks>
/// Precedence is Boundary, then Value, then Conduct, then persona, then user request. The policy carries
/// no legal or ethical text of its own; hosts supply it (see <c>IIntentPolicyProvider</c>), so
/// jurisdiction- and tenant-specific rules stay versioned content rather than hard-coded library text.
/// </remarks>
public sealed record IntentPolicy
{
    private IntentPolicy(string policyId, string version, IReadOnlyList<IntentPrinciple> principles)
    {
        PolicyId = policyId;
        Version = version;
        Principles = principles;
    }

    /// <summary>
    /// Gets the stable policy identifier.
    /// </summary>
    public string PolicyId { get; }

    /// <summary>
    /// Gets the policy version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the principles in deterministic order: kind, then priority, then id.
    /// </summary>
    public IReadOnlyList<IntentPrinciple> Principles { get; }

    /// <summary>
    /// Creates a validated policy with deterministically ordered principles.
    /// </summary>
    /// <param name="policyId">Stable policy identifier.</param>
    /// <param name="version">Policy version.</param>
    /// <param name="principles">At least one principle. Ids must be unique.</param>
    /// <returns>An immutable policy.</returns>
    public static IntentPolicy Create(string policyId, string version, IEnumerable<IntentPrinciple> principles)
    {
        ArgumentNullException.ThrowIfNull(principles);

        if (string.IsNullOrWhiteSpace(policyId))
        {
            throw new ArgumentException("Policy id is required.", nameof(policyId));
        }

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new ArgumentException("Policy version is required.", nameof(version));
        }

        var ordered = principles
            .OrderBy(x => x.Kind)
            .ThenBy(x => x.Priority)
            .ThenBy(x => x.PrincipleId, StringComparer.Ordinal)
            .ToArray();

        if (ordered.Length == 0)
        {
            throw new ArgumentException("At least one principle is required.", nameof(principles));
        }

        var duplicates = ordered
            .GroupBy(x => x.PrincipleId, StringComparer.Ordinal)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new ArgumentException($"Duplicate principle ids are not allowed: {string.Join(", ", duplicates)}", nameof(principles));
        }

        return new IntentPolicy(policyId.Trim(), version.Trim(), ordered);
    }

    /// <summary>
    /// Computes the deterministic lock hash over the policy content. Any change to an id, version,
    /// statement, kind, or priority changes the hash, so a policy change is detectable as drift.
    /// </summary>
    /// <returns>Uppercase SHA-256 hash.</returns>
    public string ComputeLockHash()
    {
        var builder = new StringBuilder();
        AppendField(builder, PolicyId);
        AppendField(builder, Version);

        foreach (var principle in Principles)
        {
            AppendField(builder, principle.PrincipleId);
            AppendField(builder, ((int)principle.Kind).ToString(System.Globalization.CultureInfo.InvariantCulture));
            AppendField(builder, principle.Priority.ToString(System.Globalization.CultureInfo.InvariantCulture));
            AppendField(builder, principle.Statement);
        }

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }

    private static void AppendField(StringBuilder builder, string value)
    {
        builder.Append(value.Length).Append(':').Append(value).Append('|');
    }
}
