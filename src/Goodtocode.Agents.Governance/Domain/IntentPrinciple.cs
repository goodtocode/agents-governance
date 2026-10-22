namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// A single, citable principle of an <see cref="IntentPolicy"/>.
/// </summary>
/// <param name="PrincipleId">Stable identifier, cited by decisions and used in compact preambles. No whitespace.</param>
/// <param name="Statement">The human-readable principle text.</param>
/// <param name="Kind">The principle kind, which sets precedence.</param>
/// <param name="Priority">Order within a kind. Lower values come first.</param>
public sealed record IntentPrinciple(
    string PrincipleId,
    string Statement,
    IntentPrincipleKind Kind,
    int Priority = 0)
{
    /// <summary>
    /// Creates a validated principle.
    /// </summary>
    /// <param name="principleId">Stable identifier without whitespace.</param>
    /// <param name="statement">The principle text.</param>
    /// <param name="kind">The principle kind.</param>
    /// <param name="priority">Order within a kind. Lower values come first.</param>
    /// <returns>A normalized principle.</returns>
    public static IntentPrinciple Create(string principleId, string statement, IntentPrincipleKind kind, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(principleId) || principleId.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException("Principle id is required and must not contain whitespace.", nameof(principleId));
        }

        if (string.IsNullOrWhiteSpace(statement))
        {
            throw new ArgumentException("Principle statement is required.", nameof(statement));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Principle kind is not defined.");
        }

        return new IntentPrinciple(principleId.Trim(), statement.Trim(), kind, priority);
    }
}
