using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class OehsFreshStartServiceTests
{
    [Fact]
    public void FirstRun_ShouldDeletePersistedDataAndProgramOutputs()
    {
        using var temp = new TempDirectory();
        var dataFile = Path.Combine(temp.DataRoot, "oehs-data.json");
        var backupFile = dataFile + ".bak";
        var tempFile = dataFile + ".tmp";

        File.WriteAllText(dataFile, "{\"old\":true}");
        File.WriteAllText(backupFile, "{\"oldBackup\":true}");
        File.WriteAllText(tempFile, "temporary");

        var outputFile = Path.Combine(temp.DesktopRoot, "Ciktilar", "Hakedisler", "old.pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllText(outputFile, "old output");

        var reset = OehsFreshStartService.EnsureFreshStart(temp.DataRoot, temp.DesktopRoot);

        Assert.True(reset);
        Assert.False(File.Exists(dataFile));
        Assert.False(File.Exists(backupFile));
        Assert.False(File.Exists(tempFile));
        Assert.False(Directory.Exists(temp.DesktopRoot));
        Assert.True(File.Exists(Path.Combine(temp.DataRoot, ".fresh-start-version")));
    }

    [Fact]
    public void SecondRun_ShouldPreserveNewData()
    {
        using var temp = new TempDirectory();

        Assert.True(OehsFreshStartService.EnsureFreshStart(temp.DataRoot, temp.DesktopRoot));

        var dataFile = Path.Combine(temp.DataRoot, "oehs-data.json");
        File.WriteAllText(dataFile, "{\"new\":true}");

        var resetAgain = OehsFreshStartService.EnsureFreshStart(temp.DataRoot, temp.DesktopRoot);

        Assert.False(resetAgain);
        Assert.True(File.Exists(dataFile));
        Assert.Contains("new", File.ReadAllText(dataFile), StringComparison.Ordinal);
    }

    private sealed class TempDirectory : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "OEHS-reset-" + Guid.NewGuid().ToString("N"));
        public string DataRoot => Path.Combine(Root, "OEHS");
        public string DesktopRoot => Path.Combine(Root, "OzelEgitimHakedisSistemi");

        public TempDirectory() => Directory.CreateDirectory(DataRoot);

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
