using System.Globalization;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Defines the visible identity and Excel-compatible per-route sequence for a monthly payment batch.
/// </summary>
public static class MonthlyProgressPaymentIdentityService
{
    public static string BuildDisplayName(
        TransportationProgramType programType,
        string tenderNumber,
        string contractNumber,
        string contractorName,
        IEnumerable<string> routeNumbers,
        DateOnly periodDate)
    {
        ArgumentNullException.ThrowIfNull(routeNumbers);

        var routes = NormalizeRouteNumbers(routeNumbers);
        if (routes.Length == 0)
            throw new ArgumentException("En az bir hat seçilmelidir.", nameof(routeNumbers));

        var programName = programType == TransportationProgramType.SpecialEducation
            ? "Özel Eğitim"
            : "Temel/Ortaöğretim";
        var contractPart = string.IsNullOrWhiteSpace(contractNumber)
            ? string.Empty
            : $" • Sözleşme {contractNumber.Trim()}";
        var routePart = string.Join("+", routes);
        var month = periodDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("tr-TR"));
        month = CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(month);

        return $"{programName} | İKN {tenderNumber.Trim()}{contractPart} | {contractorName.Trim()} | {routePart} | {month}";
    }

    public static string BuildKey(
        TransportationProgramType programType,
        string tenderNumber,
        string contractNumber,
        string contractorIdentity,
        string contractorName,
        IEnumerable<string> routeNumbers,
        DateOnly periodDate)
    {
        ArgumentNullException.ThrowIfNull(routeNumbers);
        var routes = NormalizeRouteNumbers(routeNumbers);
        var identity = OehsIdentityService.NormalizeIdentity(contractorIdentity);
        if (identity.Length == 0)
            identity = OehsIdentityService.NormalizeIdentity(contractorName);

        return string.Join(
            "|",
            programType.ToString(),
            OehsIdentityService.NormalizeIdentity(tenderNumber),
            OehsIdentityService.NormalizeIdentity(contractNumber),
            identity,
            string.Join("+", routes),
            periodDate.ToString("yyyy-MM", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Mirrors the source XLSM's route-specific Hakediş No formula: count prior
    /// months with a positive K+T payment for this exact program/contract/route,
    /// then add one. The current month and future months are excluded.
    /// </summary>
    public static int SuggestNextRouteNumber(
        IEnumerable<ProgressPaymentHistoryData> history,
        string tenderNumber,
        string contractNumber,
        string contractorIdentity,
        string routeNumber,
        TransportationProgramType programType,
        DateOnly selectedPeriodDate)
    {
        ArgumentNullException.ThrowIfNull(history);

        var monthStart = new DateOnly(selectedPeriodDate.Year, selectedPeriodDate.Month, 1);
        var targetRouteKey = OehsIdentityService.BuildRouteKey(
            tenderNumber,
            routeNumber,
            contractorIdentity,
            contractNumber,
            programType);

        var priorPositivePayments = history
            .Where(x => x.PeriodDate.HasValue && x.PeriodDate.Value < monthStart)
            .Where(x => x.Input.ProgramType == programType)
            .Where(HasCompleteRouteIdentity)
            .Where(x => string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber ?? string.Empty,
                    x.RouteNumber ?? string.Empty,
                    x.ContractorTaxOrIdentityNumber ?? string.Empty,
                    x.ContractNumber ?? string.Empty,
                    x.Input.ProgramType),
                targetRouteKey,
                StringComparison.Ordinal))
            .Count(x => ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentTotal(x) > 0m);

        return checked(priorPositivePayments + 1);
    }

    private static bool HasCompleteRouteIdentity(ProgressPaymentHistoryData payment) =>
        !string.IsNullOrWhiteSpace(payment.TenderNumber) &&
        !string.IsNullOrWhiteSpace(payment.ContractNumber) &&
        !string.IsNullOrWhiteSpace(payment.RouteNumber) &&
        !string.IsNullOrWhiteSpace(OehsIdentityService.NormalizeIdentity(payment.ContractorTaxOrIdentityNumber));

    private static string[] NormalizeRouteNumbers(IEnumerable<string> routeNumbers) =>
        routeNumbers
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
