namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Resolves the exact set of drafts for selected routes. Cached drafts for
/// deselected routes are intentionally excluded from calculation/output.
/// </summary>
public static class SelectedRouteDrafts
{
    public static IReadOnlyList<T> SelectExact<T>(
        IEnumerable<string> selectedRouteKeys,
        IReadOnlyDictionary<string, T> drafts)
    {
        ArgumentNullException.ThrowIfNull(selectedRouteKeys);
        ArgumentNullException.ThrowIfNull(drafts);

        var selected = new List<T>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var key in selectedRouteKeys)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Selected route keys cannot be empty.", nameof(selectedRouteKeys));
            if (!seen.Add(key))
                throw new InvalidOperationException($"Duplicate selected route key: '{key}'.");
            if (!drafts.TryGetValue(key, out var draft))
                throw new InvalidOperationException($"No draft exists for selected route key: '{key}'.");
            selected.Add(draft);
        }

        return selected;
    }
}
