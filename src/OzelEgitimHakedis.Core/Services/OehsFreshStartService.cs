namespace OzelEgitimHakedis.Core.Services;

public static class OehsFreshStartService
{
    // This is intentionally one-time. Changing the value creates a new explicit
    // clean-slate migration for a future release; normal launches preserve data.
    public const string CurrentResetVersion = "2026-10-01-clean-slate-01";

    public static bool EnsureFreshStart() =>
        EnsureFreshStart(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "OEHS"),
            OehsOutputPathService.DesktopRoot);

    public static bool EnsureFreshStart(string localDataRoot, string desktopRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(localDataRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(desktopRoot);

        Directory.CreateDirectory(localDataRoot);

        var markerPath = Path.Combine(localDataRoot, ".fresh-start-version");
        if (File.Exists(markerPath))
        {
            var marker = File.ReadAllText(markerPath).Trim();
            if (string.Equals(marker, CurrentResetVersion, StringComparison.Ordinal))
                return false;
        }

        var dataFilePath = Path.Combine(localDataRoot, "oehs-data.json");

        DeleteFileIfExists(dataFilePath);
        DeleteFileIfExists(dataFilePath + ".bak");
        DeleteFileIfExists(dataFilePath + ".tmp");

        // OEHS owns this desktop root. A clean-slate release removes all
        // generated archive/previous-output data beneath that application folder.
        var fullDesktopRoot = Path.GetFullPath(desktopRoot);
        if (Path.GetFileName(fullDesktopRoot)
                .Equals("OzelEgitimHakedisSistemi", StringComparison.OrdinalIgnoreCase) &&
            Directory.Exists(fullDesktopRoot))
        {
            Directory.Delete(fullDesktopRoot, recursive: true);
        }

        File.WriteAllText(markerPath, CurrentResetVersion);
        return true;
    }

    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
