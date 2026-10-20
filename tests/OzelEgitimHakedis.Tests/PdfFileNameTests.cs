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
    public void BuildProgressPdfFileName_ShouldIncludeContractPartWhenPresent()
    {
        var name = OehsFileNameService.BuildProgressPdfFileName(
            "123", "SOZ-02", "01", 3, new DateOnly(2026, 11, 30));

        Assert.Equal("OEHS_123_Sozlesme-SOZ-02_Hat-01_Hakediş-03_2026-11-30.pdf", name);
    }

    [Fact]
    public void SanitizeFileName_ShouldPreventDotTraversalAndReservedDeviceNames()
    {
        Assert.Equal("Ihale", OehsFileNameService.SanitizeFileName("..", "Ihale"));
        Assert.Equal("Ihale", OehsFileNameService.SanitizeFileName(".", "Ihale"));
        Assert.Equal("_CON", OehsFileNameService.SanitizeFileName("CON", "Ihale"));
        Assert.Equal("_CON.txt", OehsFileNameService.SanitizeFileName("CON.txt", "Ihale"));
        Assert.DoesNotContain("/", OehsFileNameService.SanitizeFileName("A/B"), StringComparison.Ordinal);
        Assert.DoesNotContain("\\", OehsFileNameService.SanitizeFileName("A\\B"), StringComparison.Ordinal);
    }

    [Fact]
    public void SanitizeFileName_ShouldLimitPathPartLength()
    {
        var input = new string('X', 200);

        var sanitized = OehsFileNameService.SanitizeFileName(input);

        Assert.True(sanitized.Length <= 80);
    }

    [Fact]
    public void SanitizeFileName_ShouldProtectWindowsDeviceNamesWithPaddingAndExtensions()
    {
        var cases = new (string Input, string Expected)[]
        {
            ("CON .txt", "_CON .txt"),
            ("con...log", "_con...log"),
            ("COM¹", "_COM¹"),
            ("LPT³.csv", "_LPT³.csv")
        };

        foreach (var item in cases)
        {
            Assert.Equal(item.Expected, OehsFileNameService.SanitizeFileName(item.Input));
        }
    }

    [Fact]
    public void SanitizeFileName_ShouldUseFallbackForEmptyOrTraversalValues()
    {
        var cases = new string?[] { null, "", "   ", ".." };

        foreach (var input in cases)
        {
            Assert.Equal("Yok", OehsFileNameService.SanitizeFileName(input, "Yok"));
        }
    }

    [Fact]
    public void BuildProgressPdfFileName_ShouldSeparateProgramAndUseMonthlyDateForProgramReports()
    {
        var specialEducation = OehsFileNameService.BuildProgressPdfFileName(
            "123", "01", 3, new DateOnly(2026, 11, 30),
            OzelEgitimHakedis.Core.Models.TransportationProgramType.SpecialEducation);
        var basicEducation = OehsFileNameService.BuildProgressPdfFileName(
            "123", "01", 3, new DateOnly(2026, 11, 30),
            OzelEgitimHakedis.Core.Models.TransportationProgramType.BasicOrSecondary);

        Assert.Contains("_OzelEgitim_", specialEducation, StringComparison.Ordinal);
        Assert.Contains("_TemelOrtaogretim_", basicEducation, StringComparison.Ordinal);
        Assert.EndsWith("_2026-11.pdf", specialEducation, StringComparison.Ordinal);
        Assert.EndsWith("_2026-11.pdf", basicEducation, StringComparison.Ordinal);
        Assert.NotEqual(specialEducation, basicEducation);
    }

    [Fact]
    public void BuildProgressPdfFileNameForContractor_ShouldIncludeStableContractorIdentityPart()
    {
        var first = OehsFileNameService.BuildProgressPdfFileNameForContractor(
            "123", "Örnek Taşımacılık", "12345678901", "01", 2,
            new DateOnly(2026, 11, 30));
        var sameIdentityDifferentDisplayName = OehsFileNameService.BuildProgressPdfFileNameForContractor(
            "123", "Örnek Taşımacılık Ltd.", "12345678901", "01", 2,
            new DateOnly(2026, 11, 30));
        var otherIdentity = OehsFileNameService.BuildProgressPdfFileNameForContractor(
            "123", "Örnek Taşımacılık", "10987654321", "01", 2,
            new DateOnly(2026, 11, 30));

        Assert.Contains("Yuklenici-", first, StringComparison.Ordinal);
        var firstIdentitySuffix = first.Split("_Hat-", 2)[0].Split('-').Last();
        var renamedIdentitySuffix = sameIdentityDifferentDisplayName.Split("_Hat-", 2)[0].Split('-').Last();
        var otherIdentitySuffix = otherIdentity.Split("_Hat-", 2)[0].Split('-').Last();

        Assert.Equal(firstIdentitySuffix, renamedIdentitySuffix);
        Assert.NotEqual(firstIdentitySuffix, otherIdentitySuffix);
    }

    [Fact]
    public void BuildProgressPdfFileName_ShouldIncludeBatchRoutesAndUseMonthlyDate()
    {
        var name = OehsFileNameService.BuildProgressPdfFileName(
            "123", "SOZ-02", "01", 2, new DateOnly(2026, 11, 30),
            OzelEgitimHakedis.Core.Models.TransportationProgramType.BasicOrSecondary,
            "01,02,03");

        Assert.Contains("_Hatlar-01,02,03_", name, StringComparison.Ordinal);
        Assert.Contains("_TemelOrtaogretim_", name, StringComparison.Ordinal);
        Assert.EndsWith("_2026-11.pdf", name, StringComparison.Ordinal);
        Assert.DoesNotContain("/", name, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildProgressPdfFileName_ShouldRejectNonPositiveProgressPaymentNumbers()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OehsFileNameService.BuildProgressPdfFileName(
                "123", "01", 0, new DateOnly(2026, 11, 30)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OehsFileNameService.BuildProgressPdfFileName(
                "123", "01", -1, new DateOnly(2026, 11, 30)));
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
