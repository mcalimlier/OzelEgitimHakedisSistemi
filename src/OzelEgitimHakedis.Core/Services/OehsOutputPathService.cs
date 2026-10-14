namespace OzelEgitimHakedis.Core.Services;

public static class OehsOutputPathService
{
    public static string DesktopRoot =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "OzelEgitimHakedisSistemi");

    public static string PdfRoot =>
        Path.Combine(DesktopRoot, "Ciktilar", "Hakedisler");

    public static string BuildPdfDirectory(string tenderNumber, string routeNumber) =>
        BuildPdfDirectory(DesktopRoot, tenderNumber, string.Empty, routeNumber);

    public static string BuildPdfDirectory(string tenderNumber, string contractNumber, string routeNumber) =>
        BuildPdfDirectory(DesktopRoot, tenderNumber, contractNumber, routeNumber);

    public static string BuildPdfDirectory(string root, string tenderNumber, string routeNumber) =>
        BuildPdfDirectory(root, tenderNumber, string.Empty, routeNumber);

    public static string BuildPdfDirectory(string root, string tenderNumber, string contractNumber, string routeNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var tender = OehsFileNameService.SanitizeFileName(tenderNumber, "Ihale");
        var route = OehsFileNameService.SanitizeFileName(routeNumber, "Hat");
        var tenderFolder = Path.Combine(root, "Ciktilar", "Hakedisler", tender);

        if (!string.IsNullOrWhiteSpace(contractNumber))
        {
            var contract = OehsFileNameService.SanitizeFileName(contractNumber, "Sozlesme");
            tenderFolder = Path.Combine(tenderFolder, $"Sozlesme-{contract}");
        }

        return Path.Combine(tenderFolder, $"Hat-{route}");
    }

    public static string BuildPdfFilePath(
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildPdfFilePath(DesktopRoot, tenderNumber, string.Empty, routeNumber, progressPaymentNumber, periodDate);

    public static string BuildPdfFilePath(
        string tenderNumber,
        string contractNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildPdfFilePath(DesktopRoot, tenderNumber, contractNumber, routeNumber, progressPaymentNumber, periodDate);

    public static string BuildPdfFilePath(
        string root,
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildPdfFilePath(root, tenderNumber, string.Empty, routeNumber, progressPaymentNumber, periodDate);

    public static string BuildPdfFilePath(
        string root,
        string tenderNumber,
        string contractNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        var directory = BuildPdfDirectory(root, tenderNumber, contractNumber, routeNumber);
        var fileName = OehsFileNameService.BuildProgressPdfFileName(
            tenderNumber,
            contractNumber,
            routeNumber,
            progressPaymentNumber,
            periodDate);

        return Path.Combine(directory, fileName);
    }
}
