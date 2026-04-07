using System.Text.Json;
using System.Text.Json.Serialization;

namespace YouTubeAdGame.Engine.Maps;

/// <summary>
/// Serialises and deserialises <see cref="MapDefinition"/> to/from JSON.
/// Uses human-readable enum names and indented output.
/// </summary>
public static class MapSerializer
{
    /// <summary>Shared options: indented, string enums, null-values omitted.</summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>Serialise a <see cref="MapDefinition"/> to a JSON string.</summary>
    public static string Serialize(MapDefinition map) =>
        JsonSerializer.Serialize(map, Options);

    /// <summary>
    /// Deserialise a <see cref="MapDefinition"/> from JSON.
    /// Returns <c>null</c> if the JSON is invalid or empty.
    /// </summary>
    public static MapDefinition? Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<MapDefinition>(json, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
