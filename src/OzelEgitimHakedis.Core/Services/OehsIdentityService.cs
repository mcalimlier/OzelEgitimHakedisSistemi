using System.Globalization;
using System.Text;

namespace OzelEgitimHakedis.Core.Services;

public static class OehsIdentityService
{
    public static string NormalizeIdentity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.Trim().ToUpperInvariant())
        {
            if (char.IsLetterOrDigit(ch))
                builder.Append(ch);
        }

        return builder.ToString();
    }

    public static string BuildContractKey(string tenderNumber, string contractNumber) =>
        string.Join("|", NormalizeText(tenderNumber), NormalizeText(contractNumber));

    public static string BuildRouteKey(
        string tenderNumber,
        string routeNumber,
        string contractorTaxOrIdentityNumber)
    {
        return string.Join(
            "|",
            NormalizeText(tenderNumber),
            NormalizeText(routeNumber),
            NormalizeIdentity(contractorTaxOrIdentityNumber));
    }

    public static string BuildRouteKey(
        string tenderNumber,
        string routeNumber,
        string contractorTaxOrIdentityNumber,
        string contractNumber)
    {
        return string.Join(
            "|",
            BuildContractKey(tenderNumber, contractNumber),
            NormalizeText(routeNumber),
            NormalizeIdentity(contractorTaxOrIdentityNumber));
    }

    public static string BuildProgressPaymentKey(
        string tenderNumber,
        string routeNumber,
        string contractorTaxOrIdentityNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        return string.Join(
            "|",
            BuildRouteKey(tenderNumber, routeNumber, contractorTaxOrIdentityNumber),
            progressPaymentNumber.ToString(CultureInfo.InvariantCulture),
            periodDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    public static string BuildProgressPaymentKey(
        string tenderNumber,
        string routeNumber,
        string contractorTaxOrIdentityNumber,
        int progressPaymentNumber,
        DateOnly periodDate,
        string contractNumber)
    {
        return string.Join(
            "|",
            BuildRouteKey(tenderNumber, routeNumber, contractorTaxOrIdentityNumber, contractNumber),
            progressPaymentNumber.ToString(CultureInfo.InvariantCulture),
            periodDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    private static string NormalizeText(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
}
