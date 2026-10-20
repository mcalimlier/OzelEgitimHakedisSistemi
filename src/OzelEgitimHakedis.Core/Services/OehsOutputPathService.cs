using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public static class OehsOutputPathService
{
    public static string DesktopRoot => OehsRuntimePaths.DesktopRoot;

    public static string PdfRoot =>
        Path.Combine(DesktopRoot, "Ciktilar", "Hakedisler");

    public static string BuildPdfDirectory(string tenderNumber, string routeNumber) =>
        BuildPdfDirectory(DesktopRoot, tenderNumber, routeNumber);

    public static string BuildPdfDirectory(string root, string tenderNumber, string routeNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var tender = OehsFileNameService.SanitizeFileName(tenderNumber, "Ihale");
        var route = OehsFileNameService.SanitizeFileName(routeNumber, "Hat");
        return Path.Combine(root, "Ciktilar", "Hakedisler", tender, $"Hat-{route}");
    }

    public static string BuildPdfDirectoryForContract(string tenderNumber, string contractNumber, string routeNumber) =>
        BuildPdfDirectoryForContract(DesktopRoot, tenderNumber, contractNumber, routeNumber);

    public static string BuildPdfDirectoryForContract(string root, string tenderNumber, string contractNumber, string routeNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        // Preserve the pre-contract-part directory convention for old/ambiguous records
        // whose contract number could not be resolved safely during migration.
        if (string.IsNullOrWhiteSpace(contractNumber))
            return BuildPdfDirectory(root, tenderNumber, routeNumber);

        var tender = OehsFileNameService.SanitizeFileName(tenderNumber, "Ihale");
        var route = OehsFileNameService.SanitizeFileName(routeNumber, "Hat");
        var contract = OehsFileNameService.SanitizeFileName(contractNumber, "Sozlesme");

        return Path.Combine(root, "Ciktilar", "Hakedisler", tender, $"Sozlesme-{contract}", $"Hat-{route}");
    }

    public static string BuildPdfDirectoryForContractor(
        string tenderNumber,
        string contractorName,
        string contractorTaxOrIdentityNumber,
        string routeNumber,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null,
        DateOnly? periodDate = null) =>
        BuildPdfDirectoryForContractor(
            DesktopRoot, tenderNumber, contractorName, contractorTaxOrIdentityNumber, routeNumber,
            programType, batchRouteNumbers, periodDate);

    public static string BuildPdfDirectoryForContractor(
        string root,
        string tenderNumber,
        string contractorName,
        string contractorTaxOrIdentityNumber,
        string routeNumber,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null,
        DateOnly? periodDate = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var tender = OehsFileNameService.SanitizeFileName(tenderNumber, "Ihale");
        var contractor = OehsFileNameService.BuildContractorFilePart(contractorName, contractorTaxOrIdentityNumber);
        var route = OehsFileNameService.SanitizeFileName(routeNumber, "Hat");
        var pathParts = new List<string> { root, "Ciktilar", "Hakedisler" };
        if (programType.HasValue)
            pathParts.Add(OehsFileNameService.BuildProgramFilePart(programType.Value));
        pathParts.Add(tender);
        pathParts.Add($"Yuklenici-{contractor}");
        if (!string.IsNullOrWhiteSpace(batchRouteNumbers) && periodDate.HasValue)
        {
            var batchRoutes = OehsFileNameService.SanitizeFileName(batchRouteNumbers, "Hatlar");
            pathParts.Add($"Aylik-{batchRoutes}_{periodDate.Value:yyyy-MM}");
        }
        pathParts.Add($"Hat-{route}");
        return Path.Combine(pathParts.ToArray());
    }

    public static string BuildContractorPdfFilePath(
        string tenderNumber,
        string contractorName,
        string contractorTaxOrIdentityNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null) =>
        BuildContractorPdfFilePath(
            DesktopRoot, tenderNumber, contractorName, contractorTaxOrIdentityNumber, routeNumber,
            progressPaymentNumber, periodDate, programType, batchRouteNumbers);

    public static string BuildContractorPdfFilePath(
        string root,
        string tenderNumber,
        string contractorName,
        string contractorTaxOrIdentityNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate,
        TransportationProgramType? programType = null,
        string? batchRouteNumbers = null)
    {
        var directory = BuildPdfDirectoryForContractor(
            root, tenderNumber, contractorName, contractorTaxOrIdentityNumber, routeNumber,
            programType, batchRouteNumbers, periodDate);
        var fileName = OehsFileNameService.BuildProgressPdfFileNameForContractor(
            tenderNumber, contractorName, contractorTaxOrIdentityNumber, routeNumber,
            progressPaymentNumber, periodDate, programType, batchRouteNumbers);
        return Path.Combine(directory, fileName);
    }

    public static string BuildPdfFilePath(
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildPdfFilePath(DesktopRoot, tenderNumber, routeNumber, progressPaymentNumber, periodDate);

    public static string BuildPdfFilePath(
        string root,
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        var directory = BuildPdfDirectory(root, tenderNumber, routeNumber);
        var fileName = OehsFileNameService.BuildProgressPdfFileName(
            tenderNumber, routeNumber, progressPaymentNumber, periodDate);
        return Path.Combine(directory, fileName);
    }

    public static string BuildContractPdfFilePath(
        string tenderNumber,
        string contractNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildContractPdfFilePath(DesktopRoot, tenderNumber, contractNumber, routeNumber, progressPaymentNumber, periodDate);

    public static string BuildContractPdfFilePath(
        string root,
        string tenderNumber,
        string contractNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        var directory = BuildPdfDirectoryForContract(root, tenderNumber, contractNumber, routeNumber);
        var fileName = OehsFileNameService.BuildProgressPdfFileName(
            tenderNumber, contractNumber, routeNumber, progressPaymentNumber, periodDate);
        return Path.Combine(directory, fileName);
    }
}
