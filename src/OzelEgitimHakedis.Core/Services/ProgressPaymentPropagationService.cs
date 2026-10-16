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
        ContractDraftData newContract,
        ProgressPaymentCalculationConstants? constants = null)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(routes);
        ArgumentNullException.ThrowIfNull(oldContract);
        ArgumentNullException.ThrowIfNull(newContract);

        var appliedConstants = constants ?? ProgressPaymentCalculationConstants.Defaults;
        var updated = history
            .Select(item => IsHistoryForContract(item, oldContract)
                ? RebuildForContract(item, newContract, appliedConstants)
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
                    StringComparison.Ordinal) &&
                item.Input.ProgramType == oldContractor.ProgramType
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
        Contractor contractor,
        ProgressPaymentCalculationConstants? constants = null)
    {
        ArgumentNullException.ThrowIfNull(history);
        ArgumentNullException.ThrowIfNull(oldRoute);
        ArgumentNullException.ThrowIfNull(newRoute);
        ArgumentNullException.ThrowIfNull(contractor);

        var appliedConstants = constants ?? ProgressPaymentCalculationConstants.Defaults;

        // Match the program and route identity explicitly. A contract-aware route
        // must never propagate into a sibling contract. Blank contract numbers are
        // legacy input and retain the historical tender/route/contractor matching.
        var newIdentity = OehsIdentityService.NormalizeIdentity(newRoute.ContractorTaxOrIdentityNumber);
        var updated = history
            .Select(item =>
            {
                var sameRoute = item.Input.ProgramType == oldRoute.ProgramType
                    && string.Equals(item.TenderNumber, oldRoute.TenderNumber, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(item.RouteNumber, oldRoute.RouteNumber, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(
                        OehsIdentityService.NormalizeIdentity(item.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(oldRoute.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal)
                    && (string.IsNullOrWhiteSpace(oldRoute.ContractNumber)
                        || string.Equals(item.ContractNumber, oldRoute.ContractNumber, StringComparison.OrdinalIgnoreCase));

                return sameRoute
                    ? RebuildForRoute(item, newRoute, contractor, newIdentity, appliedConstants)
                    : item;
            })
            .ToArray();

        EnsureNoDuplicateProgressPaymentKeys(updated);
        return updated;
    }

    private static ProgressPaymentHistoryData RebuildForContract(
        ProgressPaymentHistoryData item,
        ContractDraftData contract,
        ProgressPaymentCalculationConstants constants)
    {
        var hasGuide = contract.ProgramType == TransportationProgramType.SpecialEducation;
        var input = item.Input with
        {
            ContractAmount = contract.ContractAmount,
            IsVatExempt = contract.VatExempt,
            DailyPrice = contract.TransportDailyPrice,
            ProgramType = contract.ProgramType,
            GuideWorkDays = hasGuide ? item.Input.GuideWorkDays : 0,
            GuideDailyPrice = hasGuide ? item.Input.GuideDailyPrice : 0m,
            GuideDiscountAmount = hasGuide ? item.Input.GuideDiscountAmount : 0m,
            GuidePriceDifference = hasGuide ? item.Input.GuidePriceDifference : 0m,
            MinimumWageSupport = hasGuide ? item.Input.MinimumWageSupport : 0m
        };

        var calculation = ProgressPaymentCalculator.Calculate(input, constants);
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
        string normalizedIdentity,
        ProgressPaymentCalculationConstants constants)
    {
        var input = item.Input with
        {
            WorkDays = route.TransportWorkDays,
            DailyPrice = route.DailyPrice,
            GuideWorkDays = route.ProgramType == TransportationProgramType.BasicOrSecondary ? 0 : route.GuideWorkDays,
            GuideDailyPrice = route.ProgramType == TransportationProgramType.BasicOrSecondary
                ? 0m
                : route.GuideDailyPrice,
            ProgramType = route.ProgramType,
            GuideDiscountAmount = route.ProgramType == TransportationProgramType.BasicOrSecondary ? 0m : item.Input.GuideDiscountAmount,
            GuidePriceDifference = route.ProgramType == TransportationProgramType.BasicOrSecondary ? 0m : item.Input.GuidePriceDifference,
            MinimumWageSupport = route.ProgramType == TransportationProgramType.BasicOrSecondary ? 0m : item.Input.MinimumWageSupport
        };

        return item with
        {
            Input = input,
            Calculation = ProgressPaymentCalculator.Calculate(input, constants),
            RouteNumber = route.RouteNumber,
            RouteName = route.Name,
            TenderNumber = route.TenderNumber,
            ContractNumber = string.IsNullOrWhiteSpace(route.ContractNumber) ? item.ContractNumber : route.ContractNumber,
            ContractorName = contractor.Name,
            BankAccountNumber = contractor.BankAccountNumber,
            ContractorTaxOrIdentityNumber = normalizedIdentity
        };
    }

    private static bool IsHistoryForContract(
        ProgressPaymentHistoryData item,
        ContractDraftData contract)
    {
        if (item.Input.ProgramType != contract.ProgramType)
            return false;

        if (!string.Equals(
                item.TenderNumber?.Trim(),
                contract.TenderNumber.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Legacy rows are mapped by the WPF load migration when the association is unique.
        // A remaining blank identifier is ambiguous and must not be propagated across parts.
        return !string.IsNullOrWhiteSpace(item.ContractNumber)
            && string.Equals(item.ContractNumber.Trim(), contract.ContractNumber.Trim(), StringComparison.OrdinalIgnoreCase);
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
                item.ContractNumber ?? string.Empty,
                item.Input.ProgramType))
            .GroupBy(key => key, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateKeys.Length > 0)
            throw new ProgressPaymentPropagationException(
                "Kaynak değişikliği mevcut hakedişlerden en az ikisini aynı kimlik kombinasyonuna getiriyor.");
    }
}
