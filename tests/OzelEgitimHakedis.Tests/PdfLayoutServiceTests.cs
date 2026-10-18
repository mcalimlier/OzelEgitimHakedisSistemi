using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class PdfLayoutServiceTests
{
    private static readonly double[] IcmalHeaderWidths = { 500d, 218.11d };
    [Fact]
    public void A4PrintableWidth_ShouldLeaveDriverSafetyMargin()
    {
        Assert.True(PdfLayoutService.SafeContentWidth < PdfLayoutService.A4Width);
        Assert.Equal(660d, PdfLayoutService.SafeContentWidth);
    }

    [Fact]
    public void FitColumns_ShouldPreserveProportionsAndNeverExceedSafeWidth()
    {
        var fitted = PdfLayoutService.FitColumns(IcmalHeaderWidths);

        Assert.Equal(PdfLayoutService.SafeContentWidth, fitted.Sum(), precision: 6);
        Assert.Equal(500d / 218.11d, fitted[0] / fitted[1], precision: 10);
    }

    [Fact]
    public void OfficialFourAndFiveColumnLayouts_ShouldNeverExceedSafeWidth()
    {
        var four = new[] { 55d, 308d, 163d, 164d };
        var five = new[] { 40d, 278d, 124d, 124d, 124d };

        Assert.Equal(690d, four.Sum(), precision: 6);
        Assert.Equal(690d, five.Sum(), precision: 6);

        Assert.True(PdfLayoutService.FitColumns(four).Sum() <= PdfLayoutService.SafeContentWidth);
        Assert.True(PdfLayoutService.FitColumns(five).Sum() <= PdfLayoutService.SafeContentWidth);
    }

    [Fact]
    public void FitColumns_ShouldReturnEmptyForEmptyInput()
    {
        Assert.Empty(PdfLayoutService.FitColumns(Array.Empty<double>()));
    }

    [Fact]
    public void FitColumns_ShouldPreserveWidthsAtExactSafeBoundary()
    {
        var widths = new[] { 200d, 200d, 260d };

        var fitted = PdfLayoutService.FitColumns(widths);

        Assert.Equal(widths, fitted);
        Assert.Equal(PdfLayoutService.SafeContentWidth, fitted.Sum(), precision: 6);
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    public void FitColumns_ShouldRejectNonPositiveColumnWidths(double invalidWidth)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PdfLayoutService.FitColumns(new[] { 100d, invalidWidth, 200d }));
    }

    [Fact]
    public void FitColumns_ShouldRejectNullInput()
    {
        Assert.Throws<ArgumentNullException>(
            () => PdfLayoutService.FitColumns(null!));
    }

    [Fact]
    public void FitColumns_ShouldKeepEveryWidthPositiveForVeryWideTables()
    {
        var fitted = PdfLayoutService.FitColumns(new[] { 1_000_000d, 2_000_000d, 3_000_000d });

        Assert.All(fitted, width => Assert.True(width > 0d));
        Assert.Equal(PdfLayoutService.SafeContentWidth, fitted.Sum(), precision: 6);
    }

    [Fact]
    public void FitColumns_ShouldNotChangeAlreadyFittingColumns()
    {
        var widths = new[] { 55d, 308d, 163d, 134d };

        var fitted = PdfLayoutService.FitColumns(widths);

        Assert.Equal(widths, fitted);
    }
}
