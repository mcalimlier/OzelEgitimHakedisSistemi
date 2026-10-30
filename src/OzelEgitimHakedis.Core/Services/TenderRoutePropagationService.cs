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

    public static IReadOnlyList<RouteData> UpdateContractIdentity(
        IReadOnlyList<RouteData> routes,
        ContractDraftData oldContract,
        ContractDraftData newContract)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(oldContract);
        ArgumentNullException.ThrowIfNull(newContract);

        return routes
            .Select(route =>
            {
                var sameTender = string.Equals(
                    route.TenderNumber?.Trim(),
                    oldContract.TenderNumber.Trim(),
                    StringComparison.OrdinalIgnoreCase);
                var sameContract = !string.IsNullOrWhiteSpace(route.ContractNumber)
                    && string.Equals(route.ContractNumber.Trim(), oldContract.ContractNumber.Trim(), StringComparison.OrdinalIgnoreCase);

                if (!sameTender || !sameContract)
                    return route;

                var hasGuide = newContract.ProgramType == TransportationProgramType.SpecialEducation;
                return route with
                {
                    TenderNumber = newContract.TenderNumber.Trim(),
                    ContractNumber = newContract.ContractNumber.Trim(),
                    ProgramType = newContract.ProgramType,
                    GuideDailyPrice = hasGuide ? route.GuideDailyPrice : 0m
                };
            })
            .ToArray();
    }
}
