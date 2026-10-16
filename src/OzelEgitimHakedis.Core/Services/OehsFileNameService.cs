using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using OzelEgitimHakedis.Core.Models;

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
        DateOnly periodDate,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null) =>
        BuildProgressPdfFileName(
            tenderNumber, string.Empty, routeNumber, progressPaymentNumber, periodDate,
            programType, batchRouteNumbers);

    public static string BuildProgressPdfFileName(
        string tenderNumber,
        string contractNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(progressPaymentNumber);

        var program = programType.HasValue ? $"_{BuildProgramFilePart(programType.Value)}" : string.Empty;
        var tender = Safe(tenderNumber, "Ihale");
        var contract = string.IsNullOrWhiteSpace(contractNumber)
            ? string.Empty
            : $"_Sozlesme-{Safe(contractNumber, "Sozlesme")}";
        var batchRoutes = string.IsNullOrWhiteSpace(batchRouteNumbers)
            ? string.Empty
            : $"_Hatlar-{Safe(batchRouteNumbers, "Hatlar")}";
        var route = Safe(routeNumber, "Hat");
        var dateFormat = programType.HasValue || !string.IsNullOrWhiteSpace(batchRouteNumbers)
            ? "yyyy-MM"
            : "yyyy-MM-dd";

        return $"OEHS{program}_{tender}{contract}{batchRoutes}_Hat-{route}_Hakediş-{progressPaymentNumber.ToString("00", CultureInfo.InvariantCulture)}_{periodDate.ToString(dateFormat, CultureInfo.InvariantCulture)}.pdf";
    }

    public static string BuildProgressPdfFileNameForContractor(
        string tenderNumber,
        string contractorName,
        string contractorTaxOrIdentityNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(progressPaymentNumber);
        var program = programType.HasValue ? $"_{BuildProgramFilePart(programType.Value)}" : string.Empty;
        var tender = Safe(tenderNumber, "Ihale");
        var contractor = BuildContractorFilePart(contractorName, contractorTaxOrIdentityNumber);
        var batchRoutes = string.IsNullOrWhiteSpace(batchRouteNumbers)
            ? string.Empty
            : $"_Hatlar-{Safe(batchRouteNumbers, "Hatlar")}";
        var route = Safe(routeNumber, "Hat");
        var dateFormat = programType.HasValue || !string.IsNullOrWhiteSpace(batchRouteNumbers)
            ? "yyyy-MM"
            : "yyyy-MM-dd";
        return $"OEHS{program}_{tender}_Yuklenici-{contractor}{batchRoutes}_Hat-{route}_Hakediş-{progressPaymentNumber.ToString("00", CultureInfo.InvariantCulture)}_{periodDate.ToString(dateFormat, CultureInfo.InvariantCulture)}.pdf";
    }

    public static string BuildProgramFilePart(TransportationProgramType programType) =>
        programType == TransportationProgramType.SpecialEducation
            ? "OzelEgitim"
            : "TemelOrtaogretim";

    public static string BuildContractorFilePart(string? contractorName, string? contractorTaxOrIdentityNumber)
    {
        var name = Safe(contractorName, "Yuklenici");
        var normalizedIdentity = OehsIdentityService.NormalizeIdentity(contractorTaxOrIdentityNumber);
        var hashInput = string.IsNullOrWhiteSpace(normalizedIdentity)
            ? name.ToUpperInvariant()
            : normalizedIdentity;
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashInput)))[..10];
        return $"{name}-{hash}";
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
