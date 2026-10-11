namespace OzelEgitimHakedis.Core.Services;

public static class OehsFreshStartService
{
    // One-time clean-slate migration. Existing files are moved to a recoverable
    // backup, never permanently deleted by an automatic launch-time operation.
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

        var dataFiles = new[]
        {
            Path.Combine(localDataRoot, "oehs-data.json"),
            Path.Combine(localDataRoot, "oehs-data.json.bak"),
            Path.Combine(localDataRoot, "oehs-data.json.tmp")
        };

        var fullDesktopRoot = Path.GetFullPath(desktopRoot);
        var ownsDesktopRoot = Path.GetFileName(fullDesktopRoot)
            .Equals("OzelEgitimHakedisSistemi", StringComparison.OrdinalIgnoreCase);

        var existingDataFiles = dataFiles.Where(File.Exists).ToArray();
        var hasDesktopOutputs = ownsDesktopRoot && Directory.Exists(fullDesktopRoot);
        string? backupDataRoot = null;
        string? backupDesktopRoot = null;

        // Create the local backup first, and move rather than delete. Keep the
        // active filenames absent so the first launch starts with empty data.
        if (existingDataFiles.Length > 0)
        {
            backupDataRoot = Path.Combine(localDataRoot, "reset-backup-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(backupDataRoot);

            foreach (var file in existingDataFiles)
                File.Move(file, Path.Combine(backupDataRoot, Path.GetFileName(file)));
        }

        if (hasDesktopOutputs)
        {
            // A sibling path keeps Directory.Move on the same volume and makes
            // all generated archives/PDFs recoverable without crossing volumes.
            backupDesktopRoot = fullDesktopRoot + ".pre-reset-" + Guid.NewGuid().ToString("N");
            Directory.Move(fullDesktopRoot, backupDesktopRoot);
        }

        var markerTemporaryPath = markerPath + ".tmp";
        File.WriteAllText(markerTemporaryPath, CurrentResetVersion);
        File.Move(markerTemporaryPath, markerPath, overwrite: true);

        return true;
    }
}
