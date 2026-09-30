using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ArchiveDisplayServiceTests
{
    [Fact]
    public void ArchiveList_ShouldShowV01AndV02AsSeparateRows()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        store.Save(Archive());

        store.Save(Archive());

        var rows = ArchiveDisplayService.BuildRows(store);

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows, x => x.RevisionText == "v01");
        Assert.Contains(rows, x => x.RevisionText == "v02");
    }

    [Fact]
    public void LatestRevision_ShouldBeIdentifiedWithoutDeletingOlderRevisions()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        store.Save(Archive());
        var second = store.Save(Archive());

        var rows = ArchiveDisplayService.BuildRows(store);

        Assert.True(File.Exists(second));
        Assert.Equal("v02", rows.Last().RevisionText);
    }

    [Fact]
    public void LoadRevision_ShouldRestoreExactUserInputs()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        var expected = Archive();
        var path = store.Save(expected);

        var loaded = store.Load(path);

        Assert.Equal(expected.ProgressPayment.Input, loaded.ProgressPayment.Input);
    }

    [Fact]
    public void MalformedArchive_ShouldBecomeControlledInvalidEntry()
    {
        using var temp = new TempDirectory();
        var store = new ProgressPaymentArchiveStore(temp.Path);
        store.Save(Archive());
        File.WriteAllText(Path.Combine(temp.Path, "OEHS_broken_v01.json"), "{broken");

        var rows = ArchiveDisplayService.BuildRows(store);

        var invalid = Assert.Single(rows.Where(x => x.RevisionText == "GEÇERSİZ"));
        Assert.Equal("-", invalid.NetPayableText);
    }

    private static ProgressPaymentArchive Archive() =>
        new(
            "1.0",
            1,
            DateTime.UtcNow,
            "ActiveRecord",
            new ContractDraftData("TENDER", "SOZ-01", "İş", "Firma", 100000m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), false,
                ContractorTaxOrIdentityNumber: "12345678901"),
            new Contractor("Firma", "12345678901", BankAccountNumber: "TR0001"),
            new RouteData("01", "Hat 01", 1000m, TransportWorkDays: 20,
                TenderNumber: "TENDER", ContractorTaxOrIdentityNumber: "12345678901"),
            new ProgressPaymentHistoryData(
                Guid.NewGuid(),
                DateTime.UtcNow,
                new ProgressPaymentInput(20, 1000m, 10m, 100m, 100000m, false, 0m, 0m, 0m),
                new ProgressPaymentCalculation(
                    20000m, 90m, 20090m, 2009m, 1004.5m, 21094.5m, 190.4532m,
                    22099m, 1194.9532m, 20904.0468m, 6m,
                    new DeductionBreakdown(1004.5m, 190.4532m, 0m, 0m, 0m, 0m, 6m)),
                1, new DateOnly(2026, 10, 31), "01", "Hat 01", "İş", "Firma", "TENDER", "SOZ-01",
                ContractorTaxOrIdentityNumber: "12345678901"),
            Array.Empty<ProgressPaymentHistoryData>(),
            "OEHS_TENDER_Hat-01_Hakedis-1_2026-10-31.pdf",
            new[] { "Hakediş Raporu", "Hizmet İşleri Hakediş Raporu", "Hizmet İşleri Kabul Tutanağı", "Hakediş İcmali" });

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(), "OEHS-ARCHIVE-" + Guid.NewGuid().ToString("N"));

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
