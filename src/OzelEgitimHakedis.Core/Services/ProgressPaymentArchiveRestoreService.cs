using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentArchiveRestoreResult(
    OehsApplicationData Data,
    bool AddedAnything,
    string Message);

public static class ProgressPaymentArchiveRestoreService
{
    public static ProgressPaymentArchiveRestoreResult Restore(
        OehsApplicationData current,
        ProgressPaymentArchive archive)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(archive);

        var contracts = (current.Contracts ?? Array.Empty<ContractDraftData>()).ToList();
        var contractors = (current.Contractors ?? Array.Empty<Contractor>()).ToList();
        var routes = (current.Routes ?? Array.Empty<RouteData>()).ToList();
        var payments = (current.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>()).ToList();

        var addedContract = contracts.All(x =>
            !string.Equals(x.TenderNumber, archive.Contract.TenderNumber, StringComparison.OrdinalIgnoreCase));

        if (addedContract)
            contracts.Add(archive.Contract);

        var archiveIdentity = OehsIdentityService.NormalizeIdentity(
            archive.Contractor.TaxOrIdentityNumber);

        var addedContractor = contractors.All(x =>
            !string.Equals(
                OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                archiveIdentity,
                StringComparison.Ordinal));

        if (addedContractor)
            contractors.Add(archive.Contractor);

        var archiveRouteKey = OehsIdentityService.BuildRouteKey(
            archive.Route.TenderNumber,
            archive.Route.RouteNumber,
            archive.Route.ContractorTaxOrIdentityNumber);

        var addedRoute = routes.All(x =>
            !string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber,
                    x.RouteNumber,
                    x.ContractorTaxOrIdentityNumber),
                archiveRouteKey,
                StringComparison.Ordinal));

        if (addedRoute)
            routes.Add(archive.Route);

        var addedPayment = payments.All(x => x.Id != archive.ProgressPayment.Id);
        if (addedPayment)
            payments.Add(archive.ProgressPayment);

        var merged = new OehsApplicationData(
            contracts.FirstOrDefault(x =>
                string.Equals(
                    x.TenderNumber,
                    archive.Contract.TenderNumber,
                    StringComparison.OrdinalIgnoreCase)),
            routes,
            contractors,
            payments,
            contracts);

        var addedItems = new List<string>();
        if (addedContract) addedItems.Add("İhale");
        if (addedContractor) addedItems.Add("Yüklenici");
        if (addedRoute) addedItems.Add("Hat");
        if (addedPayment) addedItems.Add("Hakediş");

        var message = addedItems.Count == 0
            ? "Arşiv kaydı zaten aktif veride mevcut; yeni kayıt eklenmedi."
            : $"Arşiv geri yüklendi: {string.Join(", ", addedItems)}.";

        return new ProgressPaymentArchiveRestoreResult(
            merged,
            addedItems.Count > 0,
            message);
    }
}
