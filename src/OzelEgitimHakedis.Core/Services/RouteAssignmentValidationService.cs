using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Enforces that an IKN (TenderNumber) + route number pair has a single
/// contractor identity. Route numbers may be reused under different IKNs.
/// </summary>
public static class RouteAssignmentValidationService
{
    public static RouteData? FindOwnerConflict(
        IReadOnlyList<RouteData> routes,
        RouteData proposed)
    {
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(proposed);

        if (string.IsNullOrWhiteSpace(proposed.TenderNumber)
            || string.IsNullOrWhiteSpace(proposed.RouteNumber))
        {
            return null;
        }

        var proposedTender = proposed.TenderNumber.Trim();
        var proposedRoute = proposed.RouteNumber.Trim();
        var proposedOwner = OehsIdentityService.NormalizeIdentity(proposed.ContractorTaxOrIdentityNumber);

        return routes.FirstOrDefault(existing =>
            !string.IsNullOrWhiteSpace(existing.TenderNumber)
            && !string.IsNullOrWhiteSpace(existing.RouteNumber)
            && string.Equals(existing.TenderNumber.Trim(), proposedTender, StringComparison.OrdinalIgnoreCase)
            && existing.ProgramType == proposed.ProgramType
            && string.Equals(existing.RouteNumber.Trim(), proposedRoute, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(
                OehsIdentityService.NormalizeIdentity(existing.ContractorTaxOrIdentityNumber),
                proposedOwner,
                StringComparison.Ordinal));
    }
}
