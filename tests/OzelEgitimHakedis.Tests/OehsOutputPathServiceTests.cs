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
