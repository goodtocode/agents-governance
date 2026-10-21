using System.Security.Cryptography;
using System.Text;

namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Immutable profile that declares governance requirements for the four assurance pillars.
/// </summary>
public sealed record GovernancePillarsProfile(
    string PolicyProfileVersion,
    bool ObservabilityRequired,
    bool AuditabilityRequired,
    bool RepeatabilityRequired,
    bool DefensibilityRequired,
    bool IntentRequired = false)
{
    /// <summary>
    /// Creates a strict profile where all four pillars are required.
    /// </summary>
    /// <param name="policyProfileVersion">Version identifier for the governing policy profile.</param>
    /// <param name="intentRequired">True when the Agentic Intent policy must govern every inference.</param>
    /// <returns>A strict governance profile.</returns>
    public static GovernancePillarsProfile CreateStrict(string policyProfileVersion, bool intentRequired = false)
    {
        if (string.IsNullOrWhiteSpace(policyProfileVersion))
        {
            throw new ArgumentException("Policy profile version is required.", nameof(policyProfileVersion));
        }

        return new GovernancePillarsProfile(
            PolicyProfileVersion: policyProfileVersion.Trim(),
            ObservabilityRequired: true,
            AuditabilityRequired: true,
            RepeatabilityRequired: true,
            DefensibilityRequired: true,
            IntentRequired: intentRequired);
    }

    /// <summary>
    /// Returns true when every pillar is marked as required.
    /// </summary>
    /// <returns>True when all four pillars are required.</returns>
    public bool IsFullyRequired()
    {
        return ObservabilityRequired
            && AuditabilityRequired
            && RepeatabilityRequired
            && DefensibilityRequired;
    }

    /// <summary>
    /// Computes the deterministic governance lock hash used to detect drift.
    /// </summary>
    /// <returns>Uppercase SHA-256 hash of the normalized profile payload.</returns>
    public string ComputeLockHash()
    {
        if (string.IsNullOrWhiteSpace(PolicyProfileVersion))
        {
            throw new InvalidOperationException("Policy profile version must be set before computing lock hash.");
        }

        var payload = string.Join("|",
            PolicyProfileVersion.Trim(),
            ObservabilityRequired ? "1" : "0",
            AuditabilityRequired ? "1" : "0",
            RepeatabilityRequired ? "1" : "0",
            DefensibilityRequired ? "1" : "0");

        // Appended only when set so lock hashes of existing four-pillar profiles remain stable.
        if (IntentRequired)
        {
            payload += "|intent:1";
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(bytes);
    }
}
