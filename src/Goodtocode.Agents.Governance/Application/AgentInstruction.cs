using System.Security.Cryptography;
using System.Text;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// A typed Instruction: caller- or persona-supplied text stated once for a session, playbook or instantiation.
/// The Agentic Intent policy is always composed after it, so an Instruction can never outrank the policy.
/// </summary>
public sealed record AgentInstruction
{
    private AgentInstruction(string text, InstructionScope scope)
    {
        Text = text;
        Scope = scope;
    }

    /// <summary>
    /// Gets the instruction text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets how long the instruction applies.
    /// </summary>
    public InstructionScope Scope { get; }

    /// <summary>
    /// Creates a validated instruction.
    /// </summary>
    /// <param name="text">Instruction text. Must not contain a prohibited directive fragment.</param>
    /// <param name="scope">How long the instruction applies.</param>
    /// <exception cref="GovernanceValidationException">The text is empty or tries to weaken governance.</exception>
    public static AgentInstruction Create(string text, InstructionScope scope = InstructionScope.Session)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new GovernanceValidationException(
                [new GovernanceValidationIssue(nameof(Text), "Instruction text is required.")]);
        }

        var fragment = GovernanceDirectiveGuard.FindProhibitedFragment(text);
        if (fragment is not null)
        {
            throw new GovernanceValidationException(
                [new GovernanceValidationIssue(nameof(Text), $"Instruction contains a disallowed fragment: '{fragment}'.")]);
        }

        return new AgentInstruction(text.Trim(), scope);
    }

    /// <summary>
    /// Computes a deterministic hash of the instruction so a change is detectable as drift.
    /// </summary>
    /// <returns>Uppercase SHA-256 hash.</returns>
    public string ComputeHash() =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{(int)Scope}|{Text}")));
}
