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

        Assert.Equal("OEHS_123_Hat-01_Hakedis-3_2026-11-30.pdf", name);
    }

    [Fact]
    public void BuildProgressPdfFileName_ShouldSanitizeInvalidCharacters()
    {
        var name = OehsFileNameService.BuildProgressPdfFileName(
            "123/45:67", "01/02", 3, new DateOnly(2026, 11, 30));

        Assert.DoesNotContain("/", name);
        Assert.DoesNotContain(":", name);
        Assert.EndsWith("_2026-11-30.pdf", name, StringComparison.Ordinal);
    }
}
