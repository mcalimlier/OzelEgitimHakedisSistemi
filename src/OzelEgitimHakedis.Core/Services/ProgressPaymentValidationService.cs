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
        var routeNumber = route.RouteNumber.Trim();
        var identity = OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber);

        if (string.IsNullOrWhiteSpace(tenderNumber))
            errors.Add("İhale / Dosya No girilmelidir.");

        if (string.IsNullOrWhiteSpace(routeNumber))
            errors.Add("Hat No girilmelidir.");

        if (string.IsNullOrWhiteSpace(identity))
            errors.Add("Yüklenici VKN/TCKN girilmelidir.");

        var routeMatches = string.Equals(route.TenderNumber.Trim(), tenderNumber, StringComparison.OrdinalIgnoreCase)
            && string.Equals(OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber), identity, StringComparison.Ordinal)
            && !string.IsNullOrWhiteSpace(route.RouteNumber);

        if (!routeMatches)
            errors.Add("Seçilen hat, ihale ve yüklenici ilişkisiyle eşleşmiyor.");

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
                candidate.PeriodDate.Value);

            var duplicate = data.ProgressPayments?
                .Where(x => !editingExisting || x.Id != candidate.Id)
                .Any(x => string.Equals(BuildHistoryKey(x), candidateKey, StringComparison.Ordinal));

            if (duplicate == true)
                errors.Add("Mükerrer hakediş: aynı ihale/hat/yüklenici/hakediş no/dönem zaten kayıtlı.");
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
        string contractorIdentity)
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
            item.PeriodDate.Value);
    }
}
