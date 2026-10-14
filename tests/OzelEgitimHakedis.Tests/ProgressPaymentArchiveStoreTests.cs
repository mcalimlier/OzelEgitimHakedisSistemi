using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentArchiveStoreTests
{
    private static readonly string[] DocumentNames =
    {
        "Hakediş Raporu",
        "Hizmet İşleri Hakediş Raporu",
        "Hizmet İşleri Kabul Tutanağı",
        "Hakediş İcmali"
    };
    [Fact]
    public void FirstSave_ShouldCreateV01WithoutOverwriting()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var archive = CreateArchive(1);
        var firstPath = store.Save(archive);

        Assert.EndsWith("_v01.json", firstPath, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(firstPath));
    }

    [Fact]
    public void SecondSave_ShouldCreateV02AndKeepV01()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var firstPath = store.Save(CreateArchive(1));
        var secondPath = store.Save(CreateArchive(1));

        Assert.True(File.Exists(firstPath));
        Assert.True(File.Exists(secondPath));
        Assert.NotEqual(firstPath, secondPath);
        Assert.EndsWith("_v02.json", secondPath, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Archive_ShouldContainSelfContainedSnapshotFields()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var archive = CreateArchive(1);
        var path = store.Save(archive);
        var json = File.ReadAllText(path);

        Assert.Contains("\"SchemaVersion\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Revision\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Contract\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Contractor\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Route\"", json, StringComparison.Ordinal);
        Assert.Contains("\"ProgressPayment\"", json, StringComparison.Ordinal);
        Assert.Contains("\"PreviousPayments\"", json, StringComparison.Ordinal);
        Assert.Contains("\"PdfFileName\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildFileStem_ShouldSanitizeWindowsInvalidCharacters()
    {
        var archive = CreateArchive(1) with
        {
            ProgressPayment = CreateHistory(1, "IHALE:/01")
                with { TenderNumber = "IHALE:/01", RouteNumber = "01" }
        };

        var stem = ProgressPaymentArchiveStore.BuildFileStem(archive);

        Assert.DoesNotContain(":", stem, StringComparison.Ordinal);
        Assert.DoesNotContain("/", stem, StringComparison.Ordinal);
        Assert.EndsWith("_v01", stem, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_ShouldRoundTripValidArchive()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var archive = CreateArchive(1);

        var path = store.Save(archive);
        var loaded = ProgressPaymentArchiveStore.Load(path);

        Assert.Equal(archive.SchemaVersion, loaded.SchemaVersion);
        Assert.Equal(archive.Revision, loaded.Revision);
        Assert.Equal(archive.Contract, loaded.Contract);
        Assert.Equal(archive.Contractor, loaded.Contractor);
        Assert.Equal(archive.Route, loaded.Route);
        Assert.Equal(archive.ProgressPayment, loaded.ProgressPayment);
        Assert.Equal(archive.PreviousPayments, loaded.PreviousPayments);
        Assert.Equal(archive.PdfFileName, loaded.PdfFileName);
        Assert.Equal(archive.Documents, loaded.Documents);
    }

    [Fact]
    public void Load_ShouldRejectMalformedJsonWithControlledException()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var path = Path.Combine(temp.Path, "broken.json");
        File.WriteAllText(path, "{ broken");

        Assert.Throws<ProgressPaymentArchiveException>(() => ProgressPaymentArchiveStore.Load(path));
    }

    [Fact]
    public void List_ShouldIgnoreUnrelatedFilesAndOrderByRevisionKey()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var v2 = store.Save(CreateArchive(1));
        var v3 = store.Save(CreateArchive(1));
        var other = store.Save(CreateArchive(2));
        File.WriteAllText(Path.Combine(temp.Path, "not-an-archive.txt"), "ignore");

        var list = store.List();

        Assert.Equal(3, list.Count);
        Assert.Contains(v2, list.Select(x => x.FullName));
        Assert.Equal(other, list[^1].FullName);
        Assert.Contains(v3, list.Select(x => x.FullName));
    }

    private static ProgressPaymentArchive CreateArchive(int revision) =>
        new(
            "1.0",
            revision,
            DateTime.UtcNow,
            "ActiveRecord",
            new ContractDraftData(
                "IHALE-01", "SOZ-01", "Özel Eğitim Taşıma", "Firma", 100000m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), false,
                ContractorTaxOrIdentityNumber: "12345678901"),
            new Contractor("Firma", "12345678901", BankAccountNumber: "TR0001"),
            new RouteData("01", "Hat 01", 1000m, TransportWorkDays: 20, TenderNumber: "IHALE-01", ContractorTaxOrIdentityNumber: "12345678901"),
            CreateHistory(1),
            Array.Empty<ProgressPaymentHistoryData>(),
            "OEHS_IHALE-01_Hat-01_Hakedis-1_2026-10-31.pdf",
            DocumentNames);

    private static ProgressPaymentHistoryData CreateHistory(int number, string tender = "IHALE-01") =>
        new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new ProgressPaymentInput(20, 1000m, 0m, 100m, 100000m, false, 0m, 0m, 0m),
            new ProgressPaymentCalculation(20000m, 100m, 20100m, 2010m, 1005m, 21105m, 190.548m, 22110m, 1195.548m, 20914.452m, 6m,
                new DeductionBreakdown(1005m, 190.548m, 0m, 0m, 0m, 0m, 6m)),
            number,
            new DateOnly(2026, 10, 31),
            "01",
            "Hat 01",
            "Özel Eğitim Taşıma",
            "Firma",
            tender,
            "SOZ-01",
            ContractorTaxOrIdentityNumber: "12345678901");

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "OEHS-" + Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }

    [Fact]
    public void Save_ShouldRejectMismatchedRouteAndPaymentIdentity()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var archive = CreateArchive(1) with
        {
            ProgressPayment = CreateArchive(1).ProgressPayment with
            {
                RouteNumber = "02",
                RouteName = "Hat 02"
            }
        };

        var exception = Assert.Throws<ProgressPaymentArchiveException>(() => store.Save(archive));

        Assert.Contains("Hat numarası", exception.Message, StringComparison.Ordinal);
        Assert.Empty(store.List());
    }

    [Fact]
    public void SaveRevision_ShouldRejectMismatchedContractIdentityAndPreserveExistingRevision()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var firstPath = store.Save(CreateArchive(1) with
        {
            Route = CreateArchive(1).Route with { ContractNumber = "SOZ-01" }
        });
        var original = ProgressPaymentArchiveStore.Load(firstPath);
        var inconsistent = original with
        {
            Contract = original.Contract with { ContractNumber = "SOZ-02" },
            ProgressPayment = original.ProgressPayment with { ContractNumber = "SOZ-02" }
            // Route still says SOZ-01: don't allow a split identity snapshot.
        };

        var exception = Assert.Throws<ProgressPaymentArchiveException>(
            () => store.SaveRevision(firstPath, inconsistent));

        Assert.Contains("Sözleşme numarası", exception.Message, StringComparison.Ordinal);
        Assert.Single(store.List());
        Assert.Equal("SOZ-01", ProgressPaymentArchiveStore.Load(firstPath).Contract.ContractNumber);
    }

    [Fact]
    public void SaveRevision_ShouldCreateNewRevisionAndPreserveOriginalWhenRouteChanges()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var path = store.Save(CreateArchive(1));
        var original = ProgressPaymentArchiveStore.Load(path);
        var updated = original with
        {
            ArchivedAt = DateTime.UtcNow,
            Source = "ArchiveCorrection",
            Route = original.Route with
            {
                RouteNumber = "02",
                Name = "Düzeltilmiş Hat",
                TransportWorkDays = 21
            },
            ProgressPayment = original.ProgressPayment with
            {
                RouteNumber = "02",
                RouteName = "Düzeltilmiş Hat"
            }
        };

        var revisedPath = store.SaveRevision(path, updated);
        var oldVersion = ProgressPaymentArchiveStore.Load(path);
        var newVersion = ProgressPaymentArchiveStore.Load(revisedPath);

        Assert.NotEqual(path, revisedPath);
        Assert.True(File.Exists(path));
        Assert.True(File.Exists(revisedPath));
        Assert.EndsWith("_Hat-02_Hakediş-01_2026-10-31_v02.json", revisedPath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, oldVersion.Revision);
        Assert.Equal("01", oldVersion.Route.RouteNumber);
        Assert.Equal(2, newVersion.Revision);
        Assert.Equal("02", newVersion.Route.RouteNumber);
        Assert.Equal("02", newVersion.ProgressPayment.RouteNumber);
        Assert.Equal("Düzeltilmiş Hat", newVersion.Route.Name);
        Assert.Equal("Düzeltilmiş Hat", newVersion.ProgressPayment.RouteName);
        Assert.Equal(2, store.List().Count);
    }

    [Fact]
    public void SaveRevision_ShouldCreateV02AndKeepV01ForRecalculatedPayment()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var path = store.Save(CreateArchive(1));

        var saved = ProgressPaymentArchiveStore.Load(path);
        var changedInput = saved.ProgressPayment.Input with
        {
            WorkDays = 24,
            DailyPrice = 1200m,
            DiscountAmount = 25m
        };
        var recalculatedPayment = saved.ProgressPayment with
        {
            Input = changedInput,
            Calculation = ProgressPaymentCalculator.Calculate(changedInput)
        };
        var updated = saved with
        {
            ArchivedAt = DateTime.UtcNow,
            Source = "ArchiveCorrection",
            ProgressPayment = recalculatedPayment
        };

        var revisedPath = store.SaveRevision(path, updated);
        var oldVersion = ProgressPaymentArchiveStore.Load(path);
        var newVersion = ProgressPaymentArchiveStore.Load(revisedPath);

        Assert.NotEqual(path, revisedPath);
        Assert.True(File.Exists(path));
        Assert.True(File.Exists(revisedPath));
        Assert.EndsWith("_v02.json", revisedPath, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(1, oldVersion.Revision);
        Assert.Equal(20, oldVersion.ProgressPayment.Input.WorkDays);
        Assert.Equal(2, newVersion.Revision);
        Assert.Equal(24, newVersion.ProgressPayment.Input.WorkDays);
        Assert.Equal(1200m, newVersion.ProgressPayment.Input.DailyPrice);
        Assert.Equal(ProgressPaymentCalculator.Calculate(changedInput), newVersion.ProgressPayment.Calculation);
        Assert.Equal(2, store.List().Count);
    }

    [Fact]
    public void SaveRevision_ShouldRefuseIdentityCollisionAndPreserveBothArchives()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var originalPath = store.Save(CreateArchive(1));
        var original = ProgressPaymentArchiveStore.Load(originalPath);
        var other = CreateArchive(1) with
        {
            Route = new RouteData(
                "02",
                "Hat 02",
                1000m,
                TransportWorkDays: 20,
                TenderNumber: "IHALE-01",
                ContractorTaxOrIdentityNumber: "12345678901"),
            ProgressPayment = CreateHistory(1) with
            {
                RouteNumber = "02",
                RouteName = "Hat 02"
            }
        };
        var otherPath = store.Save(other);

        var attemptedUpdate = original with
        {
            Route = original.Route with { RouteNumber = "02", Name = "Çakışan Hat", TransportWorkDays = 22 },
            ProgressPayment = original.ProgressPayment with
            {
                RouteNumber = "02",
                RouteName = "Çakışan Hat"
            }
        };

        var ex = Assert.Throws<ProgressPaymentArchiveException>(
            () => store.SaveRevision(originalPath, attemptedUpdate));

        Assert.Contains("başka bir arşiv", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(originalPath));
        Assert.True(File.Exists(otherPath));
        Assert.Equal(2, store.List().Count);
        Assert.Equal("Hat 02", ProgressPaymentArchiveStore.Load(otherPath).Route.Name);
    }

    [Fact]
    public void SaveRevision_ShouldRejectMissingSourceWithoutCreatingArchive()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var missingPath = Path.Combine(temp.Path, "OEHS_IHALE_Hat-01_Hakedis-01_2026-10-31_v01.json");

        Assert.Throws<ProgressPaymentArchiveException>(() =>
            store.SaveRevision(missingPath, CreateArchive(1)));

        Assert.Empty(store.List());
        Assert.False(File.Exists(missingPath));
    }

    [Fact]
    public void SaveRevision_ShouldRejectCorrectionFromStaleRevision()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var v1Path = store.Save(CreateArchive(1));
        var v1 = ProgressPaymentArchiveStore.Load(v1Path);

        var changedInput = v1.ProgressPayment.Input with { WorkDays = 23 };
        var v2 = v1 with
        {
            ArchivedAt = DateTime.UtcNow,
            Source = "ArchiveCorrection",
            ProgressPayment = v1.ProgressPayment with
            {
                Input = changedInput,
                Calculation = ProgressPaymentCalculator.Calculate(changedInput)
            }
        };
        var v2Path = store.SaveRevision(v1Path, v2);

        var staleInput = v1.ProgressPayment.Input with { WorkDays = 30 };
        var staleUpdate = v1 with
        {
            ProgressPayment = v1.ProgressPayment with
            {
                Input = staleInput,
                Calculation = ProgressPaymentCalculator.Calculate(staleInput)
            }
        };

        var ex = Assert.Throws<ProgressPaymentArchiveException>(
            () => store.SaveRevision(v1Path, staleUpdate));

        Assert.Contains("daha yeni bir revision", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, store.List().Count);
        Assert.Equal(20, ProgressPaymentArchiveStore.Load(v1Path).ProgressPayment.Input.WorkDays);
        Assert.Equal(23, ProgressPaymentArchiveStore.Load(v2Path).ProgressPayment.Input.WorkDays);
    }

    [Fact]
    public void ThirdSave_ShouldCreateV03WithoutOverwritingEarlierRevisions()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);

        var first = store.Save(CreateArchive(1));
        var second = store.Save(CreateArchive(1));
        var third = store.Save(CreateArchive(1));

        Assert.EndsWith("_v01.json", first, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("_v02.json", second, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("_v03.json", third, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(first));
        Assert.True(File.Exists(second));
        Assert.True(File.Exists(third));
        Assert.Equal(3, store.List().Count);
    }

}
