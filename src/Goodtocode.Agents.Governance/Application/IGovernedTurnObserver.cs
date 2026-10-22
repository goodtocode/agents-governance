using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Receives the governed identity of every model turn so a host can persist it in step or chat execution metadata.
/// </summary>
public interface IGovernedTurnObserver
{
    /// <summary>
    /// Called before the model call is forwarded.
    /// </summary>
    Task OnGovernedTurnAsync(GovernedTurnRecord record, CancellationToken cancellationToken = default);
}

/// <summary>
/// Default no-op observer.
/// </summary>
public sealed class NoOpGovernedTurnObserver : IGovernedTurnObserver
{
    /// <summary>
    /// Gets the shared instance.
    /// </summary>
    public static NoOpGovernedTurnObserver Instance { get; } = new();

    /// <inheritdoc />
    public Task OnGovernedTurnAsync(GovernedTurnRecord record, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

/// <summary>
/// An observer that collects governed turns so the caller can drain them after each step and write them to
/// step metadata. Register one instance per run or session.
/// </summary>
public sealed class GovernedTurnLedger : IGovernedTurnObserver
{
    private readonly object _gate = new();
    private List<GovernedTurnRecord> _turns = [];

    /// <inheritdoc />
    public Task OnGovernedTurnAsync(GovernedTurnRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        lock (_gate)
        {
            _turns.Add(record);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns the turns collected since the last drain and clears the ledger.
    /// </summary>
    public IReadOnlyList<GovernedTurnRecord> Drain()
    {
        lock (_gate)
        {
            var drained = _turns;
            _turns = [];
            return drained;
        }
    }
}
