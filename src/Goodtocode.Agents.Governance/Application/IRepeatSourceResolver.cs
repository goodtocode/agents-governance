using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Scope a source must be resolved within.
/// </summary>
/// <param name="TenantId">Tenant the caller belongs to.</param>
/// <param name="OwnerId">Owner the caller acts as, when owner scoped.</param>
public readonly record struct RepeatScope(Guid TenantId, Guid? OwnerId = null);

/// <summary>
/// Resolves and validates the source of a Recall or Replay. Implementations must be tenant scoped, require the
/// source to have completed successfully and, for Replay, to be replay capable. Failures are reported with
/// <see cref="RepeatGuard"/> so they carry a standard <see cref="RepeatFailureCategory"/>.
/// </summary>
public interface IRepeatSourceResolver
{
    /// <summary>
    /// Resolves the validated source reference for the request.
    /// </summary>
    Task<GovernanceReference> ResolveAsync(RepeatRequest request, RepeatScope scope, CancellationToken cancellationToken = default);
}
