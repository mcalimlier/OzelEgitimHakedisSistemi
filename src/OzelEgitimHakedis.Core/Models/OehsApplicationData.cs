namespace OzelEgitimHakedis.Core.Models;

public sealed record OehsApplicationData(
    ContractDraftData? Contract,
    IReadOnlyList<RouteData> Routes,
    IReadOnlyList<Contractor> Contractors,
    IReadOnlyList<ProgressPaymentHistoryData>? ProgressPayments = null,
    IReadOnlyList<ContractDraftData>? Contracts = null,
    ProgressPaymentCalculationConstants? CalculationConstants = null,
    IReadOnlyList<ProgressPaymentCalculationConstantsChange>? CalculationConstantsHistory = null)
{
    public static OehsApplicationData Empty =>
        new(
            null,
            Array.Empty<RouteData>(),
            Array.Empty<Contractor>(),
            Array.Empty<ProgressPaymentHistoryData>(),
            Array.Empty<ContractDraftData>(),
            CalculationConstants: null,
            CalculationConstantsHistory: Array.Empty<ProgressPaymentCalculationConstantsChange>());
}

public sealed record ContractDraftData(
    string TenderNumber,
    string ContractNumber,
    string WorkName,
    string ContractorName,
    decimal ContractAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    bool VatExempt,
    DateOnly? TenderDate = null,
    DateOnly? ContractDate = null,
    string ContractorTaxOrIdentityNumber = "",
    string BankAccountNumber = "",
    int ContractWorkDays = 0,
    decimal TransportDailyPrice = 0m,
    TransportationProgramType ProgramType = TransportationProgramType.SpecialEducation)
{
    public string DisplayText => string.IsNullOrWhiteSpace(ContractorName)
        ? $"{TenderNumber} • {WorkName}"
        : $"{TenderNumber} • {ContractorName} • {WorkName}";
}

public sealed record RouteData(
    string RouteNumber,
    string Name,
    decimal DailyPrice,
    int GuideWorkDays = 0,
    decimal GuideDailyPrice = 0m,
    int TransportWorkDays = 0,
    string TenderNumber = "",
    string ContractorTaxOrIdentityNumber = "",
    string ContractNumber = "",
    TransportationProgramType ProgramType = TransportationProgramType.SpecialEducation);

public sealed record ProgressPaymentHistoryData(
    Guid Id,
    DateTime CreatedAt,
    ProgressPaymentInput Input,
    ProgressPaymentCalculation Calculation,
    int? ProgressPaymentNumber = null,
    DateOnly? PeriodDate = null,
    string? RouteNumber = null,
    string? RouteName = null,
    string? WorkName = null,
    string? ContractorName = null,
    string? TenderNumber = null,
    string? ContractNumber = null,
    DateOnly? TenderDate = null,
    DateOnly? ContractDate = null,
    DateOnly? ContractStartDate = null,
    DateOnly? ContractEndDate = null,
    string? BankAccountNumber = null,
    string? ContractorTaxOrIdentityNumber = null,
    Guid? BatchId = null,
    string? BatchDisplayName = null,
    string? BatchRouteNumbers = null);