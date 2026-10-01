using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class PdfLayoutServiceTests
{
    [Fact]
    public void A4PrintableWidth_ShouldLeaveDriverSafetyMargin()
    {
        Assert.True(PdfLayoutService.SafeContentWidth < PdfLayoutService.A4Width);
        Assert.Equal(660d, PdfLayoutService.SafeContentWidth);
    }

    [Fact]
    public void FitColumns_ShouldPreserveProportionsAndNeverExceedSafeWidth()
    {
        var fitted = PdfLayoutService.FitColumns(new[] { 500d, 218.11d });

        Assert.Equal(PdfLayoutService.SafeContentWidth, fitted.Sum(), precision: 6);
        Assert.Equal(500d / 218.11d, fitted[0] / fitted[1], precision: 10);
    }

    [Fact]
    public void FitColumns_ShouldNotChangeAlreadyFittingColumns()
    {
        var widths = new[] { 55d, 308d, 163d, 134d };

        var fitted = PdfLayoutService.FitColumns(widths);

        Assert.Equal(widths, fitted);
    }
}
