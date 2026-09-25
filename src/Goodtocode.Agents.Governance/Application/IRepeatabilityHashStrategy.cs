namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Computes deterministic repeatability hashes from raw prompt content and raw input values.
/// </summary>
/// <remarks>
/// This is the Open/Closed extension seam for hashing: core enforcement always computes hashes
/// automatically from raw values (closed for modification), while the algorithm itself can be
/// swapped by supplying a custom implementation (open for extension).
/// </remarks>
public interface IRepeatabilityHashStrategy
{
    /// <summary>
    /// Computes a deterministic hash of prompt content.
    /// </summary>
    /// <param name="promptContent">Raw prompt content. May be empty, but never null.</param>
    /// <returns>Deterministic hash string.</returns>
    string ComputePromptHash(string promptContent);

    /// <summary>
    /// Computes a deterministic hash of named raw input values.
    /// </summary>
    /// <param name="inputs">Raw input map. May be empty, but never null.</param>
    /// <returns>Deterministic hash string.</returns>
    string ComputeInputHash(IReadOnlyDictionary<string, object?> inputs);

    /// <summary>
    /// Computes a deterministic hash of governed structured output content, used to assert
    /// output identity for a Replay in addition to the existing prompt/input hashes.
    /// </summary>
    /// <param name="outputContent">Raw governed output content. May be empty, but never null.</param>
    /// <returns>Deterministic hash string.</returns>
    string ComputeOutputHash(string outputContent) => ComputePromptHash(outputContent);
}
