using System.Globalization;

namespace OzelEgitimHakedis.Core.Services;

public static class OehsFileNameService
{
    public static string BuildProgressPdfFileName(
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        if (progressPaymentNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(progressPaymentNumber));

        return $"OEHS_{Safe(tenderNumber, "Ihale")}_Hat-{Safe(routeNumber, "Hat")}_Hakedis-{progressPaymentNumber.ToString(CultureInfo.InvariantCulture)}_{periodDate:yyyy-MM-dd}.pdf";
    }

    public static string SanitizeFileName(string value, string fallback = "Kayitsiz") =>
        Safe(value, fallback);

    private static string Safe(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var invalid = Path.GetInvalidFileNameChars();
        var chars = value.Trim().Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
        return new string(chars);
    }
}
