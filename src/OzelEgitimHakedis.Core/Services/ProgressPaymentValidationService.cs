using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public static class ProgressPaymentValidationService
{
    public static IReadOnlyList<string> ValidateCreation(
        OehsApplicationData data,
        ContractDraftData contract,
        RouteData route,
        ProgressPaymentHistoryData candidate,
        bool editingExisting)
    {
        ArgumentNullException.ThrowIfNull(data);
        ArgumentNullException.ThrowIfNull(contract);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(candidate);

        var errors = new List<string>();
        var tenderNumber = contract.TenderNumber.Trim();
        var contractNumber = contract.ContractNumber.Trim();
        var routeNumber = route.RouteNumber.Trim();
        var identity = OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber);

        if (string.IsNullOrWhiteSpace(tenderNumber))
            errors.Add("İhale / Dosya No girilmelidir.");

        if (string.IsNullOrWhiteSpace(routeNumber))
            errors.Add("Hat No girilmelidir.");

        if (string.IsNullOrWhiteSpace(identity))
            errors.Add("Yüklenici VKN/TCKN girilmelidir.");

        var routeMatches = string.Equals(route.TenderNumber.Trim(), tenderNumber, StringComparison.OrdinalIgnoreCase)
            && (string.IsNullOrWhiteSpace(route.ContractNumber)
                || string.Equals(route.ContractNumber.Trim(), contractNumber, StringComparison.OrdinalIgnoreCase))
            && string.Equals(OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber), identity, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(route.RouteNumber);

        if (!routeMatches)
            errors.Add("Seçilen hat, ihale ve yüklenici ilişkisiyle eşleşmiyor.");

        var contractExists = (data.Contracts ?? Array.Empty<ContractDraftData>())
            .Append(data.Contract)
            .Where(x => x is not null)
            .Cast<ContractDraftData>()
            .Any(x => string.Equals(
                OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber),
                OehsIdentityService.BuildContractKey(tenderNumber, contractNumber),
                StringComparison.Ordinal));

        if (!contractExists)
            errors.Add("Seçilen ihale / dosya kaydı bulunamadı.");

        var routeExists = (data.Routes ?? Array.Empty<RouteData>())
            .Any(x => string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber,
                    x.RouteNumber,
                    x.ContractorTaxOrIdentityNumber,
                    x.ContractNumber),
                OehsIdentityService.BuildRouteKey(
                    route.TenderNumber,
                    route.RouteNumber,
                    route.ContractorTaxOrIdentityNumber,
                    string.IsNullOrWhiteSpace(route.ContractNumber) ? contractNumber : route.ContractNumber),
                StringComparison.Ordinal));

        if (!routeExists)
            errors.Add("Seçilen hat kaydı bulunamadı.");

        var contractorExists = (data.Contractors ?? Array.Empty<Contractor>())
            .Any(x => string.Equals(
                OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                identity,
                StringComparison.Ordinal));

        if (!contractorExists)
            errors.Add("Seçilen yüklenici firma kaydı bulunamadı.");

        if (candidate.ProgressPaymentNumber is null || candidate.ProgressPaymentNumber <= 0)
            errors.Add("Hakediş No 1 veya daha büyük olmalıdır.");

        if (candidate.PeriodDate is null)
            errors.Add("Hakediş dönem tarihi girilmelidir.");

        if (candidate.PeriodDate is not null &&
            (candidate.PeriodDate.Value < contract.StartDate || candidate.PeriodDate.Value > contract.EndDate))
        {
            errors.Add("Hakediş dönemi sözleşme tarih aralığı içinde olmalıdır.");
        }

        if (candidate.ProgressPaymentNumber is not null && candidate.PeriodDate is not null)
        {
            var candidateKey = OehsIdentityService.BuildProgressPaymentKey(
                tenderNumber,
                routeNumber,
                identity,
                candidate.ProgressPaymentNumber.Value,
                candidate.PeriodDate.Value,
                contractNumber);

            var existingPayments = data.ProgressPayments?
                .Where(x => !editingExisting || x.Id != candidate.Id)
                .ToArray() ?? Array.Empty<ProgressPaymentHistoryData>();

            var duplicate = existingPayments.Any(x =>
                string.Equals(BuildHistoryKey(x), candidateKey, StringComparison.Ordinal));

            if (duplicate)
                errors.Add("Mükerrer hakediş: aynı ihale/hat/yüklenici/hakediş no/dönem zaten kayıtlı.");

            // The reference XLSM has one monthly cell per route for K+T, transport
            // price difference, guide price difference and deductions. Saving a
            // second claim in the same route/month would overwrite that month's
            // workbook value and make the official monthly/icmal totals irreproducible.
            var routeKey = OehsIdentityService.BuildRouteKey(tenderNumber, routeNumber, identity, contractNumber);
            var sameMonthClaim = existingPayments.Any(x =>
                x.PeriodDate.HasValue &&
                x.PeriodDate.Value.Year == candidate.PeriodDate.Value.Year &&
                x.PeriodDate.Value.Month == candidate.PeriodDate.Value.Month &&
                string.Equals(
                    OehsIdentityService.BuildRouteKey(
                        x.TenderNumber ?? string.Empty,
                        x.RouteNumber ?? string.Empty,
                        x.ContractorTaxOrIdentityNumber ?? string.Empty,
                        x.ContractNumber ?? string.Empty),
                    routeKey,
                    StringComparison.Ordinal));

            if (sameMonthClaim)
                errors.Add("Aynı ihale/hat/yüklenici için aynı ayda birden fazla hakediş kaydedilemez; Excel kaynak yapısında hat başına ayda tek kayıt bulunur.");
        }

        if (candidate.PeriodDate is not null)
        {
            var latestExistingPeriod = data.ProgressPayments?
                .Where(x => !editingExisting || x.Id != candidate.Id)
                .Where(x => string.Equals(x.TenderNumber?.Trim(), tenderNumber, StringComparison.OrdinalIgnoreCase))
                .Where(x => string.Equals(x.RouteNumber?.Trim(), routeNumber, StringComparison.OrdinalIgnoreCase))
                .Where(x => string.Equals(
                    OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber ?? string.Empty),
                    identity,
                    StringComparison.Ordinal))
                .Where(x => x.PeriodDate.HasValue)
                .Select(x => x.PeriodDate!.Value)
                .DefaultIfEmpty(DateOnly.MinValue)
                .Max();

            if (latestExistingPeriod != DateOnly.MinValue &&
                candidate.PeriodDate.Value < latestExistingPeriod)
            {
                errors.Add("Hakediş dönemi önceki hakedişlerin sırasından önce olamaz.");
            }
        }

        return errors;
    }

    public static int SuggestNextProgressPaymentNumber(
        IEnumerable<ProgressPaymentHistoryData> history,
        string tenderNumber,
        string routeNumber,
        string contractorIdentity,
        string? contractNumber = null)
    {
        ArgumentNullException.ThrowIfNull(history);

        var identity = OehsIdentityService.NormalizeIdentity(contractorIdentity);
        var max = history
            .Where(x => string.Equals(x.TenderNumber?.Trim(), tenderNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals(x.RouteNumber?.Trim(), routeNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals(
                OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber ?? string.Empty),
                identity,
                StringComparison.Ordinal))
            .Where(x => string.IsNullOrWhiteSpace(contractNumber)
                || string.Equals(x.ContractNumber?.Trim(), contractNumber.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(x => x.ProgressPaymentNumber ?? 0)
            .DefaultIfEmpty(0)
            .Max();

        return checked(max + 1);
    }

    private static string BuildHistoryKey(ProgressPaymentHistoryData item)
    {
        if (!item.ProgressPaymentNumber.HasValue || !item.PeriodDate.HasValue)
            return string.Empty;

        return OehsIdentityService.BuildProgressPaymentKey(
            item.TenderNumber ?? string.Empty,
            item.RouteNumber ?? string.Empty,
            item.ContractorTaxOrIdentityNumber ?? string.Empty,
            item.ProgressPaymentNumber.Value,
            item.PeriodDate.Value,
            item.ContractNumber ?? string.Empty);
    }
}
