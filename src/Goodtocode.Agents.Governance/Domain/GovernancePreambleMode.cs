namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Controls how much of the Agentic Intent policy a Preamble carries.
/// </summary>
public enum GovernancePreambleMode
{
    /// <summary>
    /// Policy reference and principle ids only. Lowest per-turn token cost; relies on the Instruction for full text.
    /// </summary>
    Compact = 1,

    /// <summary>
    /// Policy reference plus every principle statement. Highest fidelity; counters context dilution.
    /// </summary>
    Full = 2
}
