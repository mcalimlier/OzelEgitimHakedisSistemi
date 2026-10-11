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
        BuildPdfDirectory(DesktopRoot, tenderNumber, routeNumber);

    public static string BuildPdfDirectory(string root, string tenderNumber, string routeNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        var tender = OehsFileNameService.SanitizeFileName(tenderNumber, "Ihale");
        var route = OehsFileNameService.SanitizeFileName(routeNumber, "Hat");

        return Path.Combine(root, "Ciktilar", "Hakedisler", tender, $"Hat-{route}");
    }

    public static string BuildPdfFilePath(
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate) =>
        BuildPdfFilePath(
            DesktopRoot,
            tenderNumber,
            routeNumber,
            progressPaymentNumber,
            periodDate);

    public static string BuildPdfFilePath(
        string root,
        string tenderNumber,
        string routeNumber,
        int progressPaymentNumber,
        DateOnly periodDate)
    {
        var directory = BuildPdfDirectory(root, tenderNumber, routeNumber);
        var fileName = OehsFileNameService.BuildProgressPdfFileName(
            tenderNumber,
            routeNumber,
            progressPaymentNumber,
            periodDate);

        return Path.Combine(directory, fileName);
    }
}
