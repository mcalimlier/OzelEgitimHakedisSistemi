using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class OehsOutputPathServiceTests
{
    [Fact]
    public void BuildPdfDirectory_ShouldGroupByTenderAndRoute()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var path = OehsOutputPathService.BuildPdfDirectory(
            root,
            "2026/01",
            "01");

        Assert.Equal(
            Path.Combine(root, "Ciktilar", "Hakedisler", "2026_01", "Hat-01"),
            path);
    }

    [Fact]
    public void BuildPdfDirectoryForContract_ShouldKeepLegacyFolderWhenContractNumberIsUnknown()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var path = OehsOutputPathService.BuildPdfDirectoryForContract(root, "2026/01", "", "01");

        Assert.Equal(
            Path.Combine(root, "Ciktilar", "Hakedisler", "2026_01", "Hat-01"),
            path);
    }

    [Fact]
    public void BuildPdfDirectory_ShouldSeparateContractPartsWithSameTenderAndRoute()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var first = OehsOutputPathService.BuildPdfDirectoryForContract(root, "2026/01", "SOZ-01", "01");
        var second = OehsOutputPathService.BuildPdfDirectoryForContract(root, "2026/01", "SOZ-02", "01");

        Assert.NotEqual(first, second);
        Assert.EndsWith(Path.Combine("2026_01", "Sozlesme-SOZ-01", "Hat-01"), first, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith(Path.Combine("2026_01", "Sozlesme-SOZ-02", "Hat-01"), second, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPdfFilePath_ShouldIncludeContractPart()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var path = OehsOutputPathService.BuildContractPdfFilePath(
            root, "2026/01", "SOZ-02", "01", 3, new DateOnly(2026, 11, 30));

        Assert.EndsWith(
            Path.Combine("2026_01", "Sozlesme-SOZ-02", "Hat-01",
                "OEHS_2026_01_Sozlesme-SOZ-02_Hat-01_Hakediş-03_2026-11-30.pdf"),
            path,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPdfFilePath_ShouldUseDescriptiveName()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var path = OehsOutputPathService.BuildPdfFilePath(
            root,
            "2026/01",
            "01",
            3,
            new DateOnly(2026, 11, 30));

        Assert.EndsWith(
            Path.Combine("Ciktilar", "Hakedisler", "2026_01", "Hat-01",
                "OEHS_2026_01_Hat-01_Hakediş-03_2026-11-30.pdf"),
            path,
            StringComparison.OrdinalIgnoreCase);
    }
}
