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
            .Select(item => IsHistoryForContract(item, oldContract)
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

        // Current records carry the contract number as part of the route identity.
        // Old API callers / legacy records may omit it. In that case use the legacy
        // tender-route-contractor key; when a contract number is present, never fall
        // back to the broader key and risk updating a sibling contract's history.
        var oldKey = string.IsNullOrWhiteSpace(oldRoute.ContractNumber)
            ? OehsIdentityService.BuildRouteKey(
                oldRoute.TenderNumber,
                oldRoute.RouteNumber,
                oldRoute.ContractorTaxOrIdentityNumber)
            : OehsIdentityService.BuildRouteKey(
                oldRoute.TenderNumber,
                oldRoute.RouteNumber,
                oldRoute.ContractorTaxOrIdentityNumber,
                oldRoute.ContractNumber);

        var newIdentity = OehsIdentityService.NormalizeIdentity(newRoute.ContractorTaxOrIdentityNumber);
        var updated = history
            .Select(item =>
            {
                var itemKey = string.IsNullOrWhiteSpace(oldRoute.ContractNumber)
                    ? OehsIdentityService.BuildRouteKey(
                        item.TenderNumber ?? string.Empty,
                        item.RouteNumber ?? string.Empty,
                        item.ContractorTaxOrIdentityNumber ?? string.Empty)
                    : OehsIdentityService.BuildRouteKey(
                        item.TenderNumber ?? string.Empty,
                        item.RouteNumber ?? string.Empty,
                        item.ContractorTaxOrIdentityNumber ?? string.Empty,
                        item.ContractNumber ?? string.Empty);

                return string.Equals(itemKey, oldKey, StringComparison.Ordinal)
                    ? RebuildForRoute(item, newRoute, contractor, newIdentity)
                    : item;
            })
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
            ContractNumber = route.ContractNumber,
            ContractorName = contractor.Name,
            BankAccountNumber = contractor.BankAccountNumber,
            ContractorTaxOrIdentityNumber = normalizedIdentity
        };
    }

    private static bool IsHistoryForContract(
        ProgressPaymentHistoryData item,
        ContractDraftData contract)
    {
        if (!string.Equals(
                item.TenderNumber?.Trim(),
                contract.TenderNumber.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Legacy history written before contract-part identity was introduced
        // may not have a contract number. The old application permitted only one
        // contract per tender, so that legacy row belongs to that original contract.
        return string.IsNullOrWhiteSpace(item.ContractNumber)
            || string.Equals(item.ContractNumber.Trim(), contract.ContractNumber.Trim(), StringComparison.OrdinalIgnoreCase);
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
                item.PeriodDate!.Value,
                item.ContractNumber ?? string.Empty))
            .GroupBy(key => key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateKeys.Length > 0)
            throw new ProgressPaymentPropagationException(
                "Kaynak değişikliği mevcut hakedişlerden en az ikisini aynı kimlik kombinasyonuna getiriyor.");
    }
}
