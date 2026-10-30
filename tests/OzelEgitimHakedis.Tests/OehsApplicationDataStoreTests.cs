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
    public void Save_ShouldPreserveKnownGoodBackupWhenPrimaryBecomesCorrupt()
    {
        using var temp = new TempDirectory();
        var file = Path.Combine(temp.Path, "oehs-data.json");
        var store = new OehsApplicationDataStore(file);

        store.Save(CreateData("FIRST"));
        store.Save(CreateData("SECOND"));
        var goodBackup = File.ReadAllText(file + ".bak");

        File.WriteAllText(file, "{broken");

        var exception = Assert.Throws<OehsApplicationDataStoreException>(
            () => store.Save(CreateData("THIRD")));

        Assert.Contains("yedek korunarak", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(goodBackup, File.ReadAllText(file + ".bak"));
        Assert.Equal("{broken", File.ReadAllText(file));

        // Recovery still selects the preserved FIRST backup and restores the primary.
        var recovered = store.Load();
        Assert.Equal("FIRST", recovered.Contracts?[0].TenderNumber);
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


    [Fact]
    public void SaveAndLoad_ShouldPersistCalculationConstantsAndAuditHistory()
    {
        using var temp = new TempDirectory();
        var file = Path.Combine(temp.Path, "oehs-data.json");
        var store = new OehsApplicationDataStore(file);
        var previous = ProgressPaymentCalculationConstants.Defaults;
        var current = previous with { TransportVatRate = 0.12m, StampTaxRate = 0.008m };
        var change = new ProgressPaymentCalculationConstantsChange(
            new DateTime(2026, 10, 15, 12, 30, 0, DateTimeKind.Utc),
            "Test Kullanıcısı",
            "Oran güncelleme testi",
            previous,
            current);
        var data = CreateData("TEST") with
        {
            CalculationConstants = current,
            CalculationConstantsHistory = new[] { change }
        };

        store.Save(data);
        var loaded = store.Load();

        Assert.Equal(current, loaded.CalculationConstants);
        var loadedChange = Assert.Single(loaded.CalculationConstantsHistory!);
        Assert.Equal(change.ChangedAtUtc, loadedChange.ChangedAtUtc);
        Assert.Equal(change.ChangedBy, loadedChange.ChangedBy);
        Assert.Equal(change.Reason, loadedChange.Reason);
        Assert.Equal(change.Previous, loadedChange.Previous);
        Assert.Equal(change.Current, loadedChange.Current);
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
