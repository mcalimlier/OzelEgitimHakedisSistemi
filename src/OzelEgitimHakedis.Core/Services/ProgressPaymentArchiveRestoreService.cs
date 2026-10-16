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
        if (current.Contract is not null && contracts.All(x =>
                !string.Equals(
                    OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber, x.ProgramType),
                    OehsIdentityService.BuildContractKey(current.Contract.TenderNumber, current.Contract.ContractNumber, current.Contract.ProgramType),
                    StringComparison.Ordinal)))
        {
            // Older JSON stored only the selected Contract property. Promote it
            // into the list before checking whether an archive adds a duplicate.
            contracts.Add(current.Contract);
        }

        var contractors = (current.Contractors ?? Array.Empty<Contractor>()).ToList();
        var routes = (current.Routes ?? Array.Empty<RouteData>()).ToList();
        var payments = (current.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>()).ToList();

        var addedContract = contracts.All(x =>
            !string.Equals(
                OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber, x.ProgramType),
                OehsIdentityService.BuildContractKey(archive.Contract.TenderNumber, archive.Contract.ContractNumber, archive.Contract.ProgramType),
                StringComparison.Ordinal));

        if (addedContract)
            contracts.Add(archive.Contract);

        var archiveIdentity = OehsIdentityService.NormalizeIdentity(
            archive.Contractor.TaxOrIdentityNumber);

        var addedContractor = contractors.All(x =>
            x.ProgramType != archive.Contractor.ProgramType ||
            !string.Equals(
                OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                archiveIdentity,
                StringComparison.Ordinal));

        if (addedContractor)
            contractors.Add(archive.Contractor);

        var archiveRoute = string.IsNullOrWhiteSpace(archive.Route.ContractNumber)
            ? archive.Route with { ContractNumber = archive.Contract.ContractNumber }
            : archive.Route;

        var archiveRouteKey = OehsIdentityService.BuildRouteKey(
            archiveRoute.TenderNumber,
            archiveRoute.RouteNumber,
            archiveRoute.ContractorTaxOrIdentityNumber,
            archiveRoute.ContractNumber,
            archiveRoute.ProgramType);

        var sameTenderContractCount = contracts.Count(x =>
            x.ProgramType == archiveRoute.ProgramType &&
            string.Equals(x.TenderNumber, archiveRoute.TenderNumber, StringComparison.OrdinalIgnoreCase));
        var addedRoute = routes.All(x =>
        {
            // Existing JSON written before contract-part identity has no
            // ContractNumber. It can only be assigned automatically when the
            // tender currently has exactly one contract.
            var existingContractNumber = x.ContractNumber;
            if (string.IsNullOrWhiteSpace(existingContractNumber)
                && sameTenderContractCount == 1
                && x.ProgramType == archiveRoute.ProgramType
                && string.Equals(x.TenderNumber, archiveRoute.TenderNumber, StringComparison.OrdinalIgnoreCase))
            {
                existingContractNumber = archiveRoute.ContractNumber;
            }

            return !string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber,
                    x.RouteNumber,
                    x.ContractorTaxOrIdentityNumber,
                    existingContractNumber,
                    x.ProgramType),
                archiveRouteKey,
                StringComparison.Ordinal);
        });

        if (addedRoute)
            routes.Add(archiveRoute);

        var archivePayment = string.IsNullOrWhiteSpace(archive.ProgressPayment.ContractNumber)
            ? archive.ProgressPayment with { ContractNumber = archive.Contract.ContractNumber }
            : archive.ProgressPayment;

        var existingPaymentIndex = payments.FindIndex(x => x.Id == archivePayment.Id);
        var duplicatePaymentById = existingPaymentIndex >= 0;
        var reconciledPaymentContractIdentity = false;

        if (duplicatePaymentById)
        {
            var existingPayment = payments[existingPaymentIndex];
            if (string.IsNullOrWhiteSpace(existingPayment.ContractNumber)
                && !string.IsNullOrWhiteSpace(archivePayment.ContractNumber))
            {
                // The archive and live row share a stable payment GUID, so this is
                // a safe way to resolve the missing part identity without guessing.
                payments[existingPaymentIndex] = existingPayment with
                {
                    ContractNumber = archivePayment.ContractNumber
                };
                reconciledPaymentContractIdentity = true;
            }
        }

        var duplicateBusinessKeyPayment = payments.FirstOrDefault(x =>
            x.Id != archivePayment.Id && HasSamePaymentBusinessKey(x, archivePayment));
        var duplicatePaymentByBusinessKey = duplicateBusinessKeyPayment is not null;
        var existingPaymentForConflict = duplicatePaymentById
            ? payments[existingPaymentIndex]
            : duplicateBusinessKeyPayment;
        var hasMaterialBusinessKeyConflict = existingPaymentForConflict is not null
            && HasMaterialPaymentDifferences(existingPaymentForConflict, archivePayment);

        var addedPayment = !duplicatePaymentById && !duplicatePaymentByBusinessKey;
        if (addedPayment)
            payments.Add(archivePayment);

        var merged = new OehsApplicationData(
            contracts.FirstOrDefault(x =>
                string.Equals(
                    OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber, x.ProgramType),
                    OehsIdentityService.BuildContractKey(archive.Contract.TenderNumber, archive.Contract.ContractNumber, archive.Contract.ProgramType),
                    StringComparison.Ordinal)),
            routes,
            contractors,
            payments,
            contracts);

        var addedItems = new List<string>();
        if (addedContract) addedItems.Add("İhale");
        if (addedContractor) addedItems.Add("Yüklenici");
        if (addedRoute) addedItems.Add("Hat");
        if (addedPayment) addedItems.Add("Hakediş");
        if (reconciledPaymentContractIdentity) addedItems.Add("Hakediş sözleşme bağlantısı");

        var message = addedItems.Count == 0
            ? "Arşiv kaydı zaten aktif veride mevcut veya aynı iş anahtarına sahip hakediş bulundu; yeni kayıt eklenmedi."
            : $"Arşiv geri yüklendi: {string.Join(", ", addedItems)}.";

        if (!duplicatePaymentById && duplicatePaymentByBusinessKey)
        {
            message += " Hakediş kopyası eklenmedi: aynı ihale, hat, yüklenici, hakediş numarası ve dönem anahtarında kayıt zaten var.";
        }

        if (hasMaterialBusinessKeyConflict && existingPaymentForConflict is not null)
        {
            message += " UYARI: Aynı hakediş kimliğindeki veya aynı iş anahtarındaki aktif kayıt ile arşiv kaydının girdileri ya da hesap sonuçları farklı. "
                + $"Aktif net ödeme: {FormatMoney(existingPaymentForConflict.Calculation.NetPayable)}; "
                + $"arşiv net ödeme: {FormatMoney(archivePayment.Calculation.NetPayable)}. "
                + "Aktif kayıt otomatik değiştirilmedi; kayıtları karşılaştırmadan bu farkı onaylanmış kabul etmeyin.";
        }

        return new ProgressPaymentArchiveRestoreResult(
            merged,
            addedItems.Count > 0,
            message);
    }

    private static bool HasMaterialPaymentDifferences(
        ProgressPaymentHistoryData existing,
        ProgressPaymentHistoryData candidate)
    {
        // Same business key means "do not add another row", not that the snapshots
        // are equivalent. A changed input, net amount, deductions, or applied rate
        // set may indicate a stale or manually corrected archive and must be surfaced.
        return existing.Input != candidate.Input
            || existing.Calculation.NetPayable != candidate.Calculation.NetPayable
            || existing.Calculation.GrossAmount != candidate.Calculation.GrossAmount
            || existing.Calculation.TotalDeductions != candidate.Calculation.TotalDeductions
            || existing.Calculation.AppliedConstants != candidate.Calculation.AppliedConstants;
    }

    private static string FormatMoney(decimal value) =>
        value.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " TL";

    private static bool HasSamePaymentBusinessKey(
        ProgressPaymentHistoryData existing,
        ProgressPaymentHistoryData candidate)
    {
        // Only use the composite key when all identity/period components are
        // present. Otherwise fall back to the stable GUID check above.
        if (string.IsNullOrWhiteSpace(existing.TenderNumber) ||
            string.IsNullOrWhiteSpace(candidate.TenderNumber) ||
            string.IsNullOrWhiteSpace(existing.RouteNumber) ||
            string.IsNullOrWhiteSpace(candidate.RouteNumber) ||
            string.IsNullOrWhiteSpace(existing.ContractorTaxOrIdentityNumber) ||
            string.IsNullOrWhiteSpace(candidate.ContractorTaxOrIdentityNumber) ||
            !existing.ProgressPaymentNumber.HasValue ||
            !candidate.ProgressPaymentNumber.HasValue ||
            !existing.PeriodDate.HasValue ||
            !candidate.PeriodDate.HasValue)
        {
            return false;
        }

        // A blank contract number is an unresolved legacy identity, not a wildcard.
        // Treat it as a duplicate only when both rows are legacy/blank, or when both
        // rows have the same explicit contract part. Otherwise a sibling part's archive
        // must not be blocked by an ambiguous historical row.
        var contractIdentityMatches =
            (string.IsNullOrWhiteSpace(existing.ContractNumber) && string.IsNullOrWhiteSpace(candidate.ContractNumber))
            || (!string.IsNullOrWhiteSpace(existing.ContractNumber)
                && !string.IsNullOrWhiteSpace(candidate.ContractNumber)
                && string.Equals(existing.ContractNumber.Trim(), candidate.ContractNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        return string.Equals(existing.TenderNumber.Trim(), candidate.TenderNumber.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(existing.RouteNumber.Trim(), candidate.RouteNumber.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(
                OehsIdentityService.NormalizeIdentity(existing.ContractorTaxOrIdentityNumber),
                OehsIdentityService.NormalizeIdentity(candidate.ContractorTaxOrIdentityNumber),
                StringComparison.Ordinal)
            && contractIdentityMatches
            && existing.ProgressPaymentNumber.Value == candidate.ProgressPaymentNumber.Value
            && existing.PeriodDate.Value == candidate.PeriodDate.Value;
    }
}
