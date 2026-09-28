namespace OzelEgitimHakedis.Core.Models;

public sealed record OehsApplicationData(
    ContractDraftData? Contract,
    IReadOnlyList<RouteData> Routes,
    IReadOnlyList<Contractor> Contractors,
    IReadOnlyList<ProgressPaymentHistoryData>? ProgressPayments = null)
{
    public static OehsApplicationData Empty =>
        new(null, Array.Empty<RouteData>(), Array.Empty<Contractor>());
}

public sealed record ContractDraftData(
    string TenderNumber,
    string ContractNumber,
    string WorkName,
    string ContractorName,
    decimal ContractAmount,
    DateOnly StartDate,
    DateOnly EndDate,
    bool VatExempt);

public sealed record RouteData(
    string RouteNumber,
    string Name,
    decimal DailyPrice);

public sealed record ProgressPaymentHistoryData(
    Guid Id,
    DateTime CreatedAt,
    ProgressPaymentInput Input,
    ProgressPaymentCalculation Calculation);