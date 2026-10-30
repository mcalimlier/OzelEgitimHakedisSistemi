using System.IO.Compression;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class IsolatedArchiveMigrationTests
{
    [Fact]
    public void MissingLegacyFolderDoesNotCreateAnyArchives()
    {
        using var fixture = new MigrationFixture();
        Assert.Equal(0, OehsIsolatedArchiveMigration.Ensure(fixture.Root));
        Assert.False(Directory.Exists(fixture.Target));
    }

    [Fact]
    public void CopiesRevisionBytesWithoutDeletingOriginalAndCanRunAgain()
    {
        using var fixture = new MigrationFixture();
        var original = fixture.CreateLegacy("OEHS_test_v01.json", "{\"revision\":1}");
        fixture.CreateLegacy("unrelated.txt", "keep");
        Assert.Equal(1, OehsIsolatedArchiveMigration.Ensure(fixture.Root));
        Assert.Equal(File.ReadAllBytes(original), File.ReadAllBytes(Path.Combine(fixture.Target, Path.GetFileName(original))));
        Assert.Equal(0, OehsIsolatedArchiveMigration.Ensure(fixture.Root));
        Assert.True(File.Exists(original));
        Assert.False(File.Exists(Path.Combine(fixture.Target, "unrelated.txt")));
    }

    [Fact]
    public void ConflictingRevisionIsNeverOverwritten()
    {
        using var fixture = new MigrationFixture();
        var original = fixture.CreateLegacy("OEHS_test_v01.json", "original");
        Directory.CreateDirectory(fixture.Target);
        var destination = Path.Combine(fixture.Target, Path.GetFileName(original));
        File.WriteAllText(destination, "different");
        Assert.Throws<IOException>(() => OehsIsolatedArchiveMigration.Ensure(fixture.Root));
        Assert.Equal("original", File.ReadAllText(original));
        Assert.Equal("different", File.ReadAllText(destination));
    }

    [Fact]
    public void CopiedRevisionIsIncludedByTheExistingFullBackupService()
    {
        using var fixture = new MigrationFixture();
        var legacy = fixture.CreateValidLegacy();
        OehsIsolatedArchiveMigration.Ensure(fixture.Root);
        var paths = OehsRuntimePaths.Resolve(fixture.Root, string.Empty, string.Empty, true);
        var package = Path.Combine(fixture.Root, "backup.zip");
        new OehsBackupService(paths.DataRoot, paths.DesktopRoot).Create(package);
        using var zip = ZipFile.OpenRead(package);
        Assert.NotNull(zip.GetEntry("outputs/Arsiv/Hakedisler/" + Path.GetFileName(legacy)));
    }

    [Fact]
    public void LaterRestoreDoesNotAutomaticallyReimportLegacyRevisions()
    {
        using var fixture = new MigrationFixture();
        var original = fixture.CreateLegacy("OEHS_test_v01.json", "{}");
        OehsIsolatedArchiveMigration.Ensure(fixture.Root);
        var copied = Path.Combine(fixture.Target, Path.GetFileName(original));
        File.Delete(copied); // Models a restored backup which did not contain this revision.
        Assert.Equal(0, OehsIsolatedArchiveMigration.Ensure(fixture.Root));
        Assert.True(File.Exists(original));
        Assert.False(File.Exists(copied));
    }

    private sealed class MigrationFixture : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "TEHYS-ArchiveMigration-" + Guid.NewGuid().ToString("N"));
        public string Target => OehsRuntimePaths.Resolve(Root, string.Empty, string.Empty, true).ArchiveFolderPath;
        public string CreateLegacy(string name, string content)
        {
            var folder = Path.Combine(Root, "DenemeVerisi", "Arsiv", "Hakedisler");
            Directory.CreateDirectory(folder); var path = Path.Combine(folder, name); File.WriteAllText(path, content); return path;
        }
        public string CreateValidLegacy()
        {
            var contract = new ContractDraftData("QA", "QA", "Taşıma", "Firma", 100000m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), false);
            var input = new ProgressPaymentInput(20m, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m);
            var payment = new ProgressPaymentHistoryData(Guid.NewGuid(), DateTime.UtcNow, input, ProgressPaymentCalculator.Calculate(input), 1, new DateOnly(2026, 9, 30), "01", "Hat", ContractorName: "Firma", TenderNumber: "QA", ContractNumber: "QA", ContractorTaxOrIdentityNumber: "12345678901");
            var archive = new ProgressPaymentArchive("1.0", 1, DateTime.UtcNow, "Test", contract, new Contractor("Firma", "12345678901"), new RouteData("01", "Hat", 1000m), payment, [], "test.pdf", ProgressPaymentReportService.RequiredDocumentNames);
            return new ProgressPaymentArchiveStore(Path.Combine(Root, "DenemeVerisi", "Arsiv", "Hakedisler")).Save(archive);
        }
        public void Dispose()
        {
            var full = Path.GetFullPath(Root);
            if (!full.StartsWith(Path.Combine(Path.GetFullPath(Path.GetTempPath()), "TEHYS-ArchiveMigration-"), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Unexpected test directory.");
            if (Directory.Exists(full)) Directory.Delete(full, recursive: true);
        }
    }
}
