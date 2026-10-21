namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// How long an Instruction applies. An Instruction is stated once for its scope, unlike a Preamble,
/// which precedes every model turn.
/// </summary>
public enum InstructionScope
{
    /// <summary>
    /// Applies for one chat or agent session.
    /// </summary>
    Session = 1,

    /// <summary>
    /// Applies for one playbook, across all of its stages.
    /// </summary>
    Playbook = 2,

    /// <summary>
    /// Applies for one instantiation of an agent or playbook run.
    /// </summary>
    Instantiation = 3
}
