using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class OehsRuntimePathsTests
{
    [Fact]
    public void IsolatedTestPackage_ShouldUseOnlyItsOwnDataAndOutputFolders()
    {
        var packageRoot = Path.Combine(Path.GetTempPath(), "OEHS-Package", "Release");
        var localAppData = Path.Combine(Path.GetTempPath(), "OEHS-User", "AppData");
        var desktop = Path.Combine(Path.GetTempPath(), "OEHS-User", "Desktop");

        var paths = OehsRuntimePaths.Resolve(packageRoot, localAppData, desktop, isolatedTestPackage: true);

        var testDataRoot = Path.Combine(Path.GetFullPath(packageRoot), "DenemeVerisi");
        Assert.Equal(Path.Combine(testDataRoot, "OEHS"), paths.DataRoot);
        Assert.Equal(Path.Combine(testDataRoot, "OEHS", "oehs-data.json"), paths.ApplicationDataFilePath);
        Assert.Equal(Path.Combine(testDataRoot, "Arsiv", "Hakedisler"), paths.ArchiveFolderPath);
        Assert.Equal(Path.Combine(testDataRoot, "OzelEgitimHakedisSistemi"), paths.DesktopRoot);
        Assert.Equal(Path.Combine(testDataRoot, "OEHS", "official-document-settings.json"), paths.OfficialDocumentSettingsFilePath);

        Assert.DoesNotContain(localAppData, paths.ApplicationDataFilePath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(desktop, paths.ArchiveFolderPath, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(desktop, paths.DesktopRoot, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void StandardLaunch_ShouldKeepExistingUserProfileAndDesktopLocations()
    {
        var packageRoot = Path.Combine(Path.GetTempPath(), "OEHS-Package");
        var localAppData = Path.Combine(Path.GetTempPath(), "OEHS-User", "AppData");
        var desktop = Path.Combine(Path.GetTempPath(), "OEHS-User", "Desktop");

        var paths = OehsRuntimePaths.Resolve(packageRoot, localAppData, desktop, isolatedTestPackage: false);

        var expectedDataRoot = Path.Combine(localAppData, "OEHS");
        var expectedDesktopRoot = Path.Combine(desktop, "OzelEgitimHakedisSistemi");
        Assert.Equal(expectedDataRoot, paths.DataRoot);
        Assert.Equal(Path.Combine(expectedDataRoot, "oehs-data.json"), paths.ApplicationDataFilePath);
        Assert.Equal(Path.Combine(expectedDesktopRoot, "Arsiv", "Hakedisler"), paths.ArchiveFolderPath);
        Assert.Equal(expectedDesktopRoot, paths.DesktopRoot);
        Assert.Equal(Path.Combine(expectedDataRoot, "official-document-settings.json"), paths.OfficialDocumentSettingsFilePath);
    }
}
