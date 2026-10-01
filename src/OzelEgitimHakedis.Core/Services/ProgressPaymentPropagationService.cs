using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class ProgressPaymentPropagationException : Exception
{
    public ProgressPaymentPropagationException(string message)
        : base(message)
    {
    }
}

public static class ProgressPaymentPropagationService
{
    public static IReadOnlyList<ProgressPaymentHistoryData> RecalculateForContractChange(
        IReadOnlyList<ProgressPaymentHistoryData> history,
        IReadOnlyList<RouteData> routes,
        ContractDraftData oldContract,
        ContractDraftData newContract)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(oldContract);
        ArgumentNullException.ThrowIfNull(newContract);

        var updated = history
            .Select(item => string.Equals(
                    item.TenderNumber?.Trim(),
                    oldContract.TenderNumber.Trim(),
                    StringComparison.OrdinalIgnoreCase)
                ? RebuildForContract(item, newContract)
                : item)
            .ToArray();

        EnsureNoDuplicateProgressPaymentKeys(updated);
        return updated;
    }

    public static IReadOnlyList<ProgressPaymentHistoryData> RecalculateForContractorChange(
        IReadOnlyList<ProgressPaymentHistoryData> history,
        Contractor oldContractor,
        Contractor newContractor)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(oldContractor);
        ArgumentNullException.ThrowIfNull(newContractor);

        var oldIdentity = OehsIdentityService.NormalizeIdentity(oldContractor.TaxOrIdentityNumber);
        var updated = history
            .Select(item =>
                string.Equals(
                    OehsIdentityService.NormalizeIdentity(item.ContractorTaxOrIdentityNumber ?? string.Empty),
                    oldIdentity,
                    StringComparison.Ordinal)
                    ? item with
                    {
                        ContractorName = newContractor.Name,
                        BankAccountNumber = newContractor.BankAccountNumber,
                        ContractorTaxOrIdentityNumber = OehsIdentityService.NormalizeIdentity(newContractor.TaxOrIdentityNumber)
                    }
                    : item)
            .ToArray();

        EnsureNoDuplicateProgressPaymentKeys(updated);
        return updated;
    }

    public static IReadOnlyList<ProgressPaymentHistoryData> RecalculateForRouteChange(
        IReadOnlyList<ProgressPaymentHistoryData> history,
        RouteData oldRoute,
        RouteData newRoute,
        Contractor contractor)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(oldRoute);
        ArgumentNullException.ThrowIfNull(newRoute);
        ArgumentNullException.ThrowIfNull(contractor);

        var oldKey = OehsIdentityService.BuildRouteKey(
            oldRoute.TenderNumber,
            oldRoute.RouteNumber,
            oldRoute.ContractorTaxOrIdentityNumber);

        var newIdentity = OehsIdentityService.NormalizeIdentity(newRoute.ContractorTaxOrIdentityNumber);
        var updated = history
            .Select(item =>
                string.Equals(
                    OehsIdentityService.BuildRouteKey(
                        item.TenderNumber ?? string.Empty,
                        item.RouteNumber ?? string.Empty,
                        item.ContractorTaxOrIdentityNumber ?? string.Empty),
                    oldKey,
                    StringComparison.Ordinal)
                    ? RebuildForRoute(item, newRoute, contractor, newIdentity)
                    : item)
            .ToArray();

        EnsureNoDuplicateProgressPaymentKeys(updated);
        return updated;
    }

    private static ProgressPaymentHistoryData RebuildForContract(
        ProgressPaymentHistoryData item,
        ContractDraftData contract)
    {
        var input = item.Input with
        {
            ContractAmount = contract.ContractAmount,
            IsVatExempt = contract.VatExempt,
            DailyPrice = contract.TransportDailyPrice
        };

        var calculation = ProgressPaymentCalculator.Calculate(input);
        return item with
        {
            Input = input,
            Calculation = calculation,
            WorkName = contract.WorkName,
            TenderNumber = contract.TenderNumber,
            ContractNumber = contract.ContractNumber,
            TenderDate = contract.TenderDate,
            ContractDate = contract.ContractDate,
            ContractStartDate = contract.StartDate,
            ContractEndDate = contract.EndDate
        };
    }

    private static ProgressPaymentHistoryData RebuildForRoute(
        ProgressPaymentHistoryData item,
        RouteData route,
        Contractor contractor,
        string normalizedIdentity)
    {
        var input = item.Input with
        {
            WorkDays = route.TransportWorkDays,
            DailyPrice = route.DailyPrice,
            GuideWorkDays = route.GuideWorkDays,
            GuideDailyPrice = route.GuideDailyPrice
        };

        return item with
        {
            Input = input,
            Calculation = ProgressPaymentCalculator.Calculate(input),
            RouteNumber = route.RouteNumber,
            RouteName = route.Name,
            TenderNumber = route.TenderNumber,
            ContractorName = contractor.Name,
            BankAccountNumber = contractor.BankAccountNumber,
            ContractorTaxOrIdentityNumber = normalizedIdentity
        };
    }

    private static void EnsureNoDuplicateProgressPaymentKeys(
        IReadOnlyList<ProgressPaymentHistoryData> history)
    {
        var duplicateKeys = history
            .Where(item => item.ProgressPaymentNumber.HasValue && item.PeriodDate.HasValue)
            .Select(item => OehsIdentityService.BuildProgressPaymentKey(
                item.TenderNumber ?? string.Empty,
                item.RouteNumber ?? string.Empty,
                item.ContractorTaxOrIdentityNumber ?? string.Empty,
                item.ProgressPaymentNumber!.Value,
                item.PeriodDate!.Value))
            .GroupBy(key => key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateKeys.Length > 0)
            throw new ProgressPaymentPropagationException(
                "Kaynak değişikliği mevcut hakedişlerden en az ikisini aynı kimlik kombinasyonuna getiriyor.");
    }
}
