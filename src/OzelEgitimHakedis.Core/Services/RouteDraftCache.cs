namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Holds unsaved drafts keyed by stable route identity. ResetAll deliberately
/// visits cached entries too, including entries that are not currently visible
/// in a multi-selection editor.
/// </summary>
public sealed class RouteDraftCache<T> : Dictionary<string, T> where T : class
{
    public RouteDraftCache()
        : base(StringComparer.Ordinal)
    {
    }

    public void ResetAll(Action<T> reset)
    {
        ArgumentNullException.ThrowIfNull(reset);

        foreach (var draft in Values)
            reset(draft);
    }
}
