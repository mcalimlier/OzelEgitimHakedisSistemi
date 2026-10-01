using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class OehsApplicationDataStoreTests
{
    [Fact]
    public void Save_ShouldCreateBackupOfPreviousVersion()
    {
        using var temp = new TempDirectory();
        var file = Path.Combine(temp.Path, "oehs-data.json");
        var store = new OehsApplicationDataStore(file);

        store.Save(CreateData("FIRST"));
        store.Save(CreateData("SECOND"));

        Assert.True(File.Exists(file + ".bak"));

        var backup = File.ReadAllText(file + ".bak");
        Assert.Contains("FIRST", backup, StringComparison.Ordinal);
        Assert.DoesNotContain("SECOND", backup, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_ShouldRecoverFromBackupWhenPrimaryJsonIsCorrupt()
    {
        using var temp = new TempDirectory();
        var file = Path.Combine(temp.Path, "oehs-data.json");
        var store = new OehsApplicationDataStore(file);

        store.Save(CreateData("FIRST"));
        store.Save(CreateData("SECOND"));

        File.WriteAllText(file, "{broken");

        var loaded = store.Load();

        Assert.Equal("FIRST", loaded.Contracts?[0].TenderNumber);
    }

    [Fact]
    public void Load_ShouldRestoreMissingPrimaryFromBackup()
    {
        using var temp = new TempDirectory();
        var file = Path.Combine(temp.Path, "oehs-data.json");
        var store = new OehsApplicationDataStore(file);

        store.Save(CreateData("FIRST"));
        store.Save(CreateData("SECOND"));

        File.Delete(file);

        var loaded = store.Load();

        Assert.Equal("FIRST", loaded.Contracts?[0].TenderNumber);
        Assert.True(File.Exists(file));
        Assert.Contains("FIRST", File.ReadAllText(file), StringComparison.Ordinal);
    }

    [Fact]
    public void Load_ShouldThrowControlledExceptionWhenPrimaryAndBackupAreCorrupt()
    {
        using var temp = new TempDirectory();
        var file = Path.Combine(temp.Path, "oehs-data.json");
        var store = new OehsApplicationDataStore(file);

        store.Save(CreateData("FIRST"));
        File.WriteAllText(file, "{broken");
        File.WriteAllText(file + ".bak", "{also-broken");

        Assert.Throws<OehsApplicationDataStoreException>(() => store.Load());
    }

    private static OehsApplicationData CreateData(string tender) =>
        new(
            new ContractDraftData(
                tender,
                "SOZ",
                "İş",
                "Firma",
                100m,
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 12, 31),
                false),
            Array.Empty<RouteData>(),
            Array.Empty<Contractor>(),
            Array.Empty<ProgressPaymentHistoryData>(),
            new[]
            {
                new ContractDraftData(
                    tender,
                    "SOZ",
                    "İş",
                    "Firma",
                    100m,
                    new DateOnly(2026, 1, 1),
                    new DateOnly(2026, 12, 31),
                    false)
            });

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "OEHS-STORE-" + Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
