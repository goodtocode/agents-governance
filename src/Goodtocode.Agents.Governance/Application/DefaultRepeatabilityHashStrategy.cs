using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Goodtocode.Agents.Governance.Application;

/// <summary>
/// Default deterministic SHA-256 hashing strategy for repeatability.
/// </summary>
/// <remarks>
/// This is the built-in <see cref="IRepeatabilityHashStrategy"/> used automatically when no custom
/// strategy is supplied. Most consumers never need to reference this type directly; supply a custom
/// <see cref="IRepeatabilityHashStrategy"/> to <see cref="GovernanceEnforcer"/> only if a different
/// hashing algorithm is required.
/// </remarks>
public sealed class DefaultRepeatabilityHashStrategy : IRepeatabilityHashStrategy
{
    /// <inheritdoc />
    public string ComputePromptHash(string promptContent)
    {
        return ComputeSha256(promptContent ?? string.Empty);
    }

    /// <inheritdoc />
    public string ComputeInputHash(IReadOnlyDictionary<string, object?> inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var normalizedPairs = inputs
            .OrderBy(x => x.Key, StringComparer.Ordinal)
            .Select(x => new KeyValuePair<string, string>(x.Key, Canonicalize(x.Value)))
            .ToArray();

        var normalizedJson = JsonSerializer.Serialize(normalizedPairs);
        return ComputeSha256(normalizedJson);
    }

    /// <inheritdoc />
    public string ComputeOutputHash(string outputContent)
    {
        return ComputeSha256(outputContent ?? string.Empty);
    }

    private static string Canonicalize(object? value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        return CanonicalizeElement(doc.RootElement);
    }

    private static string CanonicalizeElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => CanonicalizeObject(element),
            JsonValueKind.Array => CanonicalizeArray(element),
            _ => element.GetRawText()
        };
    }

    private static string CanonicalizeObject(JsonElement element)
    {
        var properties = element.EnumerateObject()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => $"\"{JsonEncodedText.Encode(p.Name)}\":{CanonicalizeElement(p.Value)}");

        return $"{{{string.Join(",", properties)}}}";
    }

    private static string CanonicalizeArray(JsonElement element)
    {
        var items = element.EnumerateArray()
            .Select(CanonicalizeElement);

        return $"[{string.Join(",", items)}]";
    }

    private static string ComputeSha256(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(hash);
    }
}
