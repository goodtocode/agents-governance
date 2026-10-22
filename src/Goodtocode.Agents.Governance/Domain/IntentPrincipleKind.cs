namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Classifies an <see cref="IntentPrinciple"/>. The kind sets precedence when principles or layers conflict:
/// <see cref="Boundary"/> outranks <see cref="Value"/>, which outranks <see cref="Conduct"/>.
/// </summary>
public enum IntentPrincipleKind
{
    /// <summary>
    /// Law and safety. Non-negotiable: no persona, caller, or user request can override it.
    /// </summary>
    Boundary = 1,

    /// <summary>
    /// Ethics and positive values (for example growth, collaboration, respect).
    /// </summary>
    Value = 2,

    /// <summary>
    /// Role-, persona-, or tone-specific conduct. Outranked by boundaries and values.
    /// </summary>
    Conduct = 3
}
