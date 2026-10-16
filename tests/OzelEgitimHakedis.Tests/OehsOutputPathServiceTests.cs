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
    public void BuildPdfDirectoryForContractor_ShouldSeparateContractorsWithoutExposingTaxIdentity()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var first = OehsOutputPathService.BuildPdfDirectoryForContractor(
            root, "2026/01", "Firma A", "11111111111", "01");
        var second = OehsOutputPathService.BuildPdfDirectoryForContractor(
            root, "2026/01", "Firma B", "22222222222", "01");

        Assert.NotEqual(first, second);
        Assert.Contains("Firma A-", first, StringComparison.Ordinal);
        Assert.DoesNotContain("11111111111", first, StringComparison.Ordinal);
        Assert.DoesNotContain("Sozlesme", first, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildContractorPdfFilePath_ShouldNotExposeInternalContractNumberOrFullIdentity()
    {
        var root = Path.Combine(@"C:\Users\Test\Desktop", "OzelEgitimHakedisSistemi");

        var path = OehsOutputPathService.BuildContractorPdfFilePath(
            root, "2026/01", "Firma A", "11111111111", "01", 3, new DateOnly(2026, 11, 30));

        Assert.DoesNotContain("11111111111", path, StringComparison.Ordinal);
        Assert.DoesNotContain("Sozlesme", path, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Yuklenici-Firma A-", path, StringComparison.Ordinal);
        Assert.EndsWith("Hakediş-03_2026-11-30.pdf", path, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPdfDirectoryForContractor_ShouldSeparateProgramAndMonthlyBatch()
    {
        var root = Path.Combine(@"C:\\Users\\Test\\Desktop", "OzelEgitimHakedisSistemi");

        var special = OehsOutputPathService.BuildPdfDirectoryForContractor(
            root, "2026/01", "Firma A", "11111111111", "01",
            OzelEgitimHakedis.Core.Models.TransportationProgramType.SpecialEducation,
            "Q1+Q3", new DateOnly(2026, 10, 31));
        var basic = OehsOutputPathService.BuildPdfDirectoryForContractor(
            root, "2026/01", "Firma A", "11111111111", "01",
            OzelEgitimHakedis.Core.Models.TransportationProgramType.BasicOrSecondary,
            "Q1+Q3", new DateOnly(2026, 10, 31));

        Assert.NotEqual(special, basic);
        Assert.Contains(Path.Combine("OzelEgitim", "2026_01"), special, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Path.Combine("Aylik-Q1+Q3_2026-10", "Hat-01"), special, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Path.Combine("TemelOrtaogretim", "2026_01"), basic, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildContractorPdfFilePath_ShouldIncludeProgramBatchAndMonth()
    {
        var root = Path.Combine(@"C:\\Users\\Test\\Desktop", "OzelEgitimHakedisSistemi");

        var path = OehsOutputPathService.BuildContractorPdfFilePath(
            root, "2026/01", "Firma A", "11111111111", "01", 3,
            new DateOnly(2026, 11, 30),
            OzelEgitimHakedis.Core.Models.TransportationProgramType.SpecialEducation,
            "Q1+Q3");

        Assert.Contains("OzelEgitim", path, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Hatlar-Q1+Q3", path, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("Hakediş-03_2026-11.pdf", path, StringComparison.OrdinalIgnoreCase);
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
