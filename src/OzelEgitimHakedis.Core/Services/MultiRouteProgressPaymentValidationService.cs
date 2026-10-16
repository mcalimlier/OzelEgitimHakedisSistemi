namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Prevents a multi-route payment from multiplying single-form monetary inputs
/// across route records without an explicit per-route allocation.
/// </summary>
public static class MultiRouteProgressPaymentValidationService
{
    public static IReadOnlyList<string> FindSharedMonetaryInputs(
        decimal discount,
        decimal priceDifference,
        decimal taxDebt,
        decimal socialSecurityDebt,
        decimal minimumWageSupport,
        decimal inspectionPenalty,
        decimal guideDiscount,
        decimal guidePriceDifference)
    {
        var fields = new List<string>();
        AddIfNonZero(discount, "Taşıma İskonto", fields);
        AddIfNonZero(priceDifference, "Taşıma Fiyat Farkı", fields);
        AddIfNonZero(guideDiscount, "Rehber İskonto", fields);
        AddIfNonZero(guidePriceDifference, "Rehber Fiyat Farkı", fields);
        AddIfNonZero(taxDebt, "Vergi Borcu", fields);
        AddIfNonZero(socialSecurityDebt, "SGK Borcu", fields);
        AddIfNonZero(minimumWageSupport, "Asgari Ücret Desteği", fields);
        AddIfNonZero(inspectionPenalty, "Denetim Cezası", fields);
        return fields;
    }

    private static void AddIfNonZero(decimal value, string label, ICollection<string> fields)
    {
        if (value != 0m)
            fields.Add(label);
    }
}
