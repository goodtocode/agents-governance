namespace Goodtocode.Agents.Governance.Domain;

/// <summary>
/// Identity of the Instruction, policy and Preamble that governed one model turn.
/// </summary>
/// <param name="Turn">The kind of turn.</param>
/// <param name="PolicyId">The applied policy id, or null when no policy resolved.</param>
/// <param name="PolicyVersion">The applied policy version.</param>
/// <param name="PolicyHash">The applied policy lock hash.</param>
/// <param name="InstructionApplied">Whether an Instruction was applied.</param>
/// <param name="PreambleApplied">Whether a Preamble was applied.</param>
/// <param name="InstructionHash">The applied Instruction hash, or null when none applied.</param>
public sealed record GovernedTurnRecord(
    GovernedTurnKind Turn,
    string? PolicyId,
    string? PolicyVersion,
    string? PolicyHash,
    bool InstructionApplied,
    bool PreambleApplied,
    string? InstructionHash = null);
