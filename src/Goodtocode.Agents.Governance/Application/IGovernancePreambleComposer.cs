using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Builds the short, deterministic per-turn Preamble that re-asserts Agentic Intent before every model turn.
/// </summary>
/// <remarks>
/// An Instruction (see <see cref="EvaluationGovernancePromptContext.SystemInstruction"/>) is applied once per
/// session, run, or instantiation. A Preamble is applied before every chat message and before every model-backed
/// Collect, Evaluate, or Record action.
/// </remarks>
public interface IGovernancePreambleComposer
{
    /// <summary>
    /// Composes the Preamble for one model turn.
    /// </summary>
    /// <param name="request">Preamble request.</param>
    /// <returns>Preamble text and traceability metadata.</returns>
    GovernancePreambleContext Compose(GovernancePreambleRequest request);
}

/// <summary>
/// Preamble composition input.
/// </summary>
public sealed class GovernancePreambleRequest
{
    /// <summary>
    /// Gets or sets the Agentic Intent policy to re-assert. Required.
    /// </summary>
    public IntentPolicy? Policy { get; init; }

    /// <summary>
    /// Gets or sets the kind of turn the Preamble precedes.
    /// </summary>
    public GovernedTurnKind Turn { get; init; } = GovernedTurnKind.Chat;

    /// <summary>
    /// Gets or sets how much of the policy the Preamble carries.
    /// </summary>
    public GovernancePreambleMode Mode { get; init; } = GovernancePreambleMode.Compact;
}

/// <summary>
/// Preamble composition output.
/// </summary>
public sealed class GovernancePreambleContext
{
    /// <summary>
    /// Gets or sets the Preamble text to prepend to the turn. Never render it as a chat message.
    /// </summary>
    public string Text { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets traceability metadata under <c>governance.preamble.*</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; init; } =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
