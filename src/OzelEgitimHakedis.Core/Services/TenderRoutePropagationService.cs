using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public static class TenderRoutePropagationService
{
    public static IReadOnlyList<RouteData> UpdateTenderNumber(
        IReadOnlyList<RouteData> routes,
        string oldTenderNumber,
        string newTenderNumber)
    {
        ArgumentNullException.ThrowIfNull(routes);

        var oldTender = oldTenderNumber.Trim();

        return routes
            .Select(route =>
                string.Equals(route.TenderNumber?.Trim(), oldTender, StringComparison.OrdinalIgnoreCase)
                    ? route with { TenderNumber = newTenderNumber.Trim() }
                    : route)
            .ToArray();
    }
}
