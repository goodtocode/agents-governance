using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Describes the scope for which an Agentic Intent policy is resolved.
/// </summary>
/// <param name="TenantId">The tenant whose policy applies.</param>
/// <param name="Jurisdiction">Optional jurisdiction code (for example a country or region) whose legal rules apply.</param>
public sealed record IntentPolicyRequest(Guid TenantId, string? Jurisdiction = null);

/// <summary>
/// Supplies the versioned Agentic Intent policy for a tenant and jurisdiction. Hosts implement this so
/// legal and ethical content stays versioned policy data rather than hard-coded library text.
/// </summary>
public interface IIntentPolicyProvider
{
    /// <summary>
    /// Resolves the effective policy.
    /// </summary>
    /// <param name="request">Tenant and optional jurisdiction.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The effective policy, or null when none applies.</returns>
    Task<IntentPolicy?> ResolveAsync(IntentPolicyRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Provider that always returns one fixed policy. Suitable for single-tenant hosts, samples, and tests.
/// </summary>
/// <param name="policy">The fixed policy.</param>
public sealed class StaticIntentPolicyProvider(IntentPolicy policy) : IIntentPolicyProvider
{
    private readonly IntentPolicy _policy = policy ?? throw new ArgumentNullException(nameof(policy));

    /// <inheritdoc />
    public Task<IntentPolicy?> ResolveAsync(IntentPolicyRequest request, CancellationToken cancellationToken = default)
        => Task.FromResult<IntentPolicy?>(_policy);
}
