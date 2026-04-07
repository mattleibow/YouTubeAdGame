using Microsoft.JSInterop;
using YouTubeAdGame.Engine.Maps;

namespace YouTubeAdGame.Web.Services;

/// <summary>
/// Persists and retrieves <see cref="MapDefinition"/> instances in browser
/// <c>localStorage</c> via JS interop.  Maps are stored as JSON strings.
/// </summary>
public sealed class CustomMapService(IJSRuntime js)
{
    // ── Named saved maps ─────────────────────────────────────────────────────

    /// <summary>Return all map names saved in browser storage.</summary>
    public async Task<List<string>> ListNamesAsync()
    {
        try
        {
            var result = await js.InvokeAsync<List<string>>("gameStorage.listMapNames");
            return result ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <summary>Save a map under a given name.</summary>
    public async Task SaveAsync(string name, MapDefinition map)
    {
        string json = MapSerializer.Serialize(map);
        await js.InvokeVoidAsync("gameStorage.saveMap", name, json);
    }

    /// <summary>Load a named map.  Returns <c>null</c> if not found or invalid.</summary>
    public async Task<MapDefinition?> LoadAsync(string name)
    {
        string? json = await js.InvokeAsync<string?>("gameStorage.loadMap", name);
        return json is null ? null : MapSerializer.Deserialize(json);
    }

    /// <summary>Delete a named map from storage.</summary>
    public async Task DeleteAsync(string name) =>
        await js.InvokeVoidAsync("gameStorage.removeMap", name);

    // ── Active custom map ─────────────────────────────────────────────────────

    /// <summary>
    /// Persist the map that will be used for <see cref="Engine.Core.GameMode.Custom"/>.
    /// </summary>
    public async Task SaveActiveMapAsync(MapDefinition map)
    {
        string json = MapSerializer.Serialize(map);
        await js.InvokeVoidAsync("gameStorage.saveActiveMap", json);
    }

    /// <summary>Load the active custom map. Returns <c>null</c> if none is stored.</summary>
    public async Task<MapDefinition?> LoadActiveMapAsync()
    {
        string? json = await js.InvokeAsync<string?>("gameStorage.loadActiveMap");
        return json is null ? null : MapSerializer.Deserialize(json);
    }

    // ── JSON download ─────────────────────────────────────────────────────────

    /// <summary>Trigger a browser file download of the given map as JSON.</summary>
    public async Task ExportJsonAsync(MapDefinition map, string? filename = null)
    {
        string json = MapSerializer.Serialize(map);
        string name = (filename ?? map.Name) is { Length: > 0 } n
            ? n.Replace(' ', '_').ToLowerInvariant() + ".json"
            : "custom_map.json";
        await js.InvokeVoidAsync("downloadJson", name, json);
    }
}
