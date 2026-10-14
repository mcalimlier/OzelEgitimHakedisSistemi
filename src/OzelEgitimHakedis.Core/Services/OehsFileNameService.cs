using System.Globalization;

namespace OzelEgitimHakedis.Core.Services;

public static class OehsFileNameService
{
    private const int MaxPartLength = 80;

    private static readonly HashSet<string> ReservedWindowsNames = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    public static string BuildProgressPdfFileName(
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildProgressPdfFileName(tenderNumber, string.Empty, routeNumber, progressPaymentNumber, periodDate);

    public static string BuildProgressPdfFileName(
        string tenderNumber,
        string contractNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(progressPaymentNumber);

        var tender = Safe(tenderNumber, "Ihale");
        var contract = string.IsNullOrWhiteSpace(contractNumber)
            ? string.Empty
            : $"_Sozlesme-{Safe(contractNumber, "Sozlesme")}";
        var route = Safe(routeNumber, "Hat");

        return $"OEHS_{tender}{contract}_Hat-{route}_Hakediş-{progressPaymentNumber.ToString("00", CultureInfo.InvariantCulture)}_{periodDate:yyyy-MM-dd}.pdf";
    }

    public static string SanitizeFileName(string? value, string fallback = "Kayitsiz") =>
        Safe(value, fallback);

    private static string Safe(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var invalid = Path.GetInvalidFileNameChars();
        var chars = value
            .Trim()
            .Select(ch => invalid.Contains(ch) || ch == '/' || ch == '\\' ? '_' : ch)
            .ToArray();

        var sanitized = new string(chars).Trim().TrimEnd('.', ' ');
        if (string.IsNullOrWhiteSpace(sanitized) || sanitized is "." or "..")
            return fallback;

        var deviceStem = sanitized.Split('.', 2)[0];
        if (ReservedWindowsNames.Contains(deviceStem))
            sanitized = "_" + sanitized;

        if (sanitized.Length > MaxPartLength)
            sanitized = sanitized[..MaxPartLength].TrimEnd('.', ' ');

        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }
}
