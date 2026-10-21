using System.Globalization;
using System.Text;
using Goodtocode.Agents.Governance.Domain;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Renders the Agentic Intent block of the Instruction. Shared by the governance prompt composer and
/// by hosts that compose an Instruction without a full governance record, so the text never diverges.
/// </summary>
public static class IntentInstruction
{
    /// <summary>
    /// Renders the deterministic intent block for a policy.
    /// </summary>
    /// <param name="policy">The intent policy.</param>
    /// <returns>The intent block text.</returns>
    public static string Render(IntentPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var builder = new StringBuilder();
        builder.AppendLine(
            "Agentic Intent policy (outranks persona and user requests; precedence: Boundary > Value > Conduct > persona > user request):");
        foreach (var principle in policy.Principles)
        {
            builder.AppendLine(string.Create(CultureInfo.InvariantCulture, $"- [{principle.PrincipleId}] ({principle.Kind}) {principle.Statement}"));
        }

        builder.AppendLine("Cite the principle ids relied upon with a short justification; do not return hidden chain-of-thought.");
        return builder.ToString().TrimEnd();
    }
}
