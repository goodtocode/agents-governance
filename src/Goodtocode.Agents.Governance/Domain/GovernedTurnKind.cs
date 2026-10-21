namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// The kind of model turn a Preamble precedes. A Preamble is applied before every model turn;
/// an Instruction is applied once per session, run, or instantiation.
/// </summary>
public enum GovernedTurnKind
{
    /// <summary>
    /// A user message in a chat session.
    /// </summary>
    Chat = 1,

    /// <summary>
    /// A model-backed Collect action.
    /// </summary>
    Collect = 2,

    /// <summary>
    /// A model-backed Evaluate action.
    /// </summary>
    Evaluate = 3,

    /// <summary>
    /// A model-backed Record action.
    /// </summary>
    Record = 4
}
