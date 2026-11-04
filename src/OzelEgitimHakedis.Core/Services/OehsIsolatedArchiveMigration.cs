namespace OzelEgitimHakedis.Core.Services;

/// <summary>Copies legacy trial archives into the output tree covered by full backups.</summary>
public static class OehsIsolatedArchiveMigration
{
    public static int Ensure(string packageDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageDirectory);
        var root = Path.GetFullPath(packageDirectory);
        var legacy = Path.Combine(root, "DenemeVerisi", "Arsiv", "Hakedisler");
        if (!Directory.Exists(legacy)) return 0;
        var marker = Path.Combine(legacy, ".copied-to-backup-root-v1");
        if (File.Exists(marker)) return 0;
        var target = OehsRuntimePaths.Resolve(root, string.Empty, string.Empty, isolatedTestPackage: true).ArchiveFolderPath;
        var copied = 0;
        foreach (var file in new ProgressPaymentArchiveStore(legacy).List())
        {
            var destination = Path.Combine(target, file.Name);
            if (File.Exists(destination))
            {
                if (!File.ReadAllBytes(file.FullName).AsSpan().SequenceEqual(File.ReadAllBytes(destination)))
                    throw new IOException($"Deneme arşivinde aynı adlı farklı dosyalar var; hiçbir dosyanın üzerine yazılmadı. Eski: {file.FullName} — Yeni: {destination}");
                continue;
            }
            Directory.CreateDirectory(target);
            File.Copy(file.FullName, destination, overwrite: false);
            copied++;
        }
        // Originals remain available for recovery; repeated launches are harmless.
        // Keep this receipt outside restored roots so a deliberate restore cannot
        // accidentally re-import legacy revisions that its backup did not contain.
        File.WriteAllText(marker, "Legacy trial archives copied into full-backup output tree.");
        return copied;
    }
}
