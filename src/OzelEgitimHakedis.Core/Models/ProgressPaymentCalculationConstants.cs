namespace OzelEgitimHakedis.Core.Models;

/// <summary>
/// Calculation-rate values migrated from the source XLSM defaults.
/// Values are fractions internally (10% = 0.10).
/// </summary>
public sealed record ProgressPaymentCalculationConstants(
    decimal TransportVatRate,
    decimal GuideVatRate,
    decimal TransportWithholdingShare,
    decimal GuideWithholdingShare,
    decimal StampTaxRate,
    decimal AdditionalGuaranteeRate)
{
    public static ProgressPaymentCalculationConstants Defaults => new(
        TransportVatRate: 0.10m,
        GuideVatRate: 0.20m,
        TransportWithholdingShare: 0.50m,
        GuideWithholdingShare: 0.90m,
        StampTaxRate: 0.00948m,
        AdditionalGuaranteeRate: 0.06m);

    public bool TryValidate(out string error)
    {
        var issues = new List<string>();
        AddRangeError(TransportVatRate, "Taşıma KDV oranı", issues);
        AddRangeError(GuideVatRate, "Rehber KDV oranı", issues);
        AddRangeError(TransportWithholdingShare, "Taşıma tevkifat payı", issues);
        AddRangeError(GuideWithholdingShare, "Rehber tevkifat payı", issues);
        AddRangeError(StampTaxRate, "Damga vergisi oranı", issues);
        AddRangeError(AdditionalGuaranteeRate, "Ek kesin teminat oranı", issues);
        error = string.Join(" ", issues);
        return issues.Count == 0;
    }

    private static void AddRangeError(decimal value, string label, ICollection<string> issues)
    {
        if (value < 0m || value > 1m)
            issues.Add($"{label} %0 ile %100 arasında olmalıdır.");
    }
}

public sealed record ProgressPaymentCalculationConstantsChange(
    DateTime ChangedAtUtc,
    string ChangedBy,
    string Reason,
    ProgressPaymentCalculationConstants Previous,
    ProgressPaymentCalculationConstants Current);
