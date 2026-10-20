using System;
using System.IO;

namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Resolves OEHS persistence and output locations. A package with the explicit marker
/// runs in an isolated folder so trial data never mixes with the user's normal OEHS data.
/// </summary>
public static class OehsRuntimePaths
{
    public const string TestPackageMarkerFileName = "OEHS_DENEME_PAKETI.flag";

    private static readonly OehsRuntimePathSet Current = Resolve(
        AppContext.BaseDirectory,
        GetLocalApplicationDataDirectory(),
        GetDesktopDirectory(),
        File.Exists(Path.Combine(AppContext.BaseDirectory, TestPackageMarkerFileName)));

    public static string DataRoot => Current.DataRoot;
    public static string ApplicationDataFilePath => Current.ApplicationDataFilePath;
    public static string ArchiveFolderPath => Current.ArchiveFolderPath;
    public static string DesktopRoot => Current.DesktopRoot;
    public static string OfficialDocumentSettingsFilePath => Current.OfficialDocumentSettingsFilePath;
    public static bool IsIsolatedTestPackage =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, TestPackageMarkerFileName));

    public static OehsRuntimePathSet Resolve(
        string baseDirectory,
        string localApplicationDataDirectory,
        string desktopDirectory,
        bool isolatedTestPackage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        var normalizedBaseDirectory = Path.GetFullPath(baseDirectory);
        if (isolatedTestPackage)
        {
            var testDataRoot = Path.Combine(normalizedBaseDirectory, "DenemeVerisi");
            var dataRoot = Path.Combine(testDataRoot, "OEHS");
            return new OehsRuntimePathSet(
                DataRoot: dataRoot,
                ApplicationDataFilePath: Path.Combine(dataRoot, "oehs-data.json"),
                ArchiveFolderPath: Path.Combine(testDataRoot, "Arsiv", "Hakedisler"),
                DesktopRoot: Path.Combine(testDataRoot, "OzelEgitimHakedisSistemi"),
                OfficialDocumentSettingsFilePath: Path.Combine(dataRoot, "official-document-settings.json"));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(localApplicationDataDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(desktopDirectory);

        var normalDataRoot = Path.Combine(localApplicationDataDirectory, "OEHS");
        var normalDesktopRoot = Path.Combine(desktopDirectory, "OzelEgitimHakedisSistemi");
        return new OehsRuntimePathSet(
            DataRoot: normalDataRoot,
            ApplicationDataFilePath: Path.Combine(normalDataRoot, "oehs-data.json"),
            ArchiveFolderPath: Path.Combine(normalDesktopRoot, "Arsiv", "Hakedisler"),
            DesktopRoot: normalDesktopRoot,
            OfficialDocumentSettingsFilePath: Path.Combine(normalDataRoot, "official-document-settings.json"));
    }

    private static string GetLocalApplicationDataDirectory()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (!string.IsNullOrWhiteSpace(path))
            return path;

        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share");
    }

    private static string GetDesktopDirectory()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        if (!string.IsNullOrWhiteSpace(path))
            return path;

        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop");
    }
}

public sealed record OehsRuntimePathSet(
    string DataRoot,
    string ApplicationDataFilePath,
    string ArchiveFolderPath,
    string DesktopRoot,
    string OfficialDocumentSettingsFilePath);
