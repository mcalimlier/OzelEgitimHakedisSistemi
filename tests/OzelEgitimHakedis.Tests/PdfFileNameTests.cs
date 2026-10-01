using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class PdfFileNameTests
{
    [Fact]
    public void BuildProgressPdfFileName_ShouldUseExactDescriptivePattern()
    {
        var name = OehsFileNameService.BuildProgressPdfFileName(
            "123", "01", 3, new DateOnly(2026, 11, 30));

        Assert.Equal("OEHS_123_Hat-01_Hakediş-03_2026-11-30.pdf", name);
    }

    [Fact]
    public void SanitizeFileName_ShouldPreventDotTraversalAndReservedDeviceNames()
    {
        Assert.Equal("Ihale", OehsFileNameService.SanitizeFileName("..", "Ihale"));
        Assert.Equal("Ihale", OehsFileNameService.SanitizeFileName(".", "Ihale"));
        Assert.Equal("_CON", OehsFileNameService.SanitizeFileName("CON", "Ihale"));
        Assert.DoesNotContain("/", OehsFileNameService.SanitizeFileName("A/B"));
        Assert.DoesNotContain("\\", OehsFileNameService.SanitizeFileName("A\\B"));
    }

    [Fact]
    public void SanitizeFileName_ShouldLimitPathPartLength()
    {
        var input = new string('X', 200);

        var sanitized = OehsFileNameService.SanitizeFileName(input);

        Assert.True(sanitized.Length <= 80);
    }

    [Fact]
    public void BuildProgressPdfFileName_ShouldSanitizeInvalidCharacters()
    {
        var name = OehsFileNameService.BuildProgressPdfFileName(
            "123/45:67", "01/02", 3, new DateOnly(2026, 11, 30));

        Assert.DoesNotContain("/", name, StringComparison.Ordinal);
        Assert.DoesNotContain(":", name, StringComparison.Ordinal);
        Assert.EndsWith("_2026-11-30.pdf", name, StringComparison.Ordinal);
    }
}
