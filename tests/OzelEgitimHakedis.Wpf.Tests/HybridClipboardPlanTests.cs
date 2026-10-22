using System;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class HybridClipboardPlanTests
{
    [Fact]
    public void TwoByTwoPaste_ShouldCreateIndependentSelectedRouteEdits()
    {
        var columns = new[] { "WorkDays", "PriceDifference" };
        var accepted = HybridClipboardPlan.TryCreate(
            "16\t1.234,50\r\n20\t-22,75\r\n", columns,
            firstRow: 0, firstColumn: 0, rowCount: 2,
            (_, _) => true, out var cells, out var error);
        Assert.True(accepted, error);
        Assert.Equal(4, cells.Count);
        Assert.Equal(new HybridClipboardPlan.Cell(0, "WorkDays", 16m), cells[0]);
        Assert.Equal(new HybridClipboardPlan.Cell(0, "PriceDifference", 1234.50m), cells[1]);
        Assert.Equal(new HybridClipboardPlan.Cell(1, "WorkDays", 20m), cells[2]);
        Assert.Equal(new HybridClipboardPlan.Cell(1, "PriceDifference", -22.75m), cells[3]);
    }

    [Theory]
    [InlineData("16\tgeçersiz", "geçerli tr-TR")]
    [InlineData("-1\t0", "negatif")]
    [InlineData("16\t12.50", "geçerli tr-TR")]
    [InlineData("16\t1E5", "geçerli tr-TR")]
    public void InvalidPaste_ShouldRejectWholeRange(string clipboard, string expectedError)
    {
        Assert.False(HybridClipboardPlan.TryCreate(
            clipboard, new[] { "WorkDays", "PriceDifference" },
            0, 0, 2, (_, _) => true, out var cells, out var error));
        Assert.Empty(cells);
        Assert.Contains(expectedError, error, StringComparison.Ordinal);
    }

    [Fact]
    public void Paste_ShouldRejectReadOnlyPricesAndNonSelectedRows()
    {
        Assert.False(HybridClipboardPlan.TryCreate(
            "100", new[] { "DailyPrice" }, 0, 0, 2,
            (_, _) => true, out var readOnly, out var readonlyError));
        Assert.Empty(readOnly);
        Assert.Contains("salt okunur", readonlyError, StringComparison.Ordinal);

        Assert.False(HybridClipboardPlan.TryCreate(
            "16\n20", new[] { "WorkDays" }, 0, 0, 1,
            (_, _) => true, out var excess, out var boundaryError));
        Assert.Empty(excess);
        Assert.Contains("sınırlarını", boundaryError, StringComparison.Ordinal);
    }

    [Fact]
    public void Paste_ShouldRejectDisabledPerRouteFieldsAndNonNumericCheckboxColumns()
    {
        Assert.False(HybridClipboardPlan.TryCreate(
            "15", new[] { "MinimumWageSupport" }, 0, 0, 1,
            (_, _) => false, out var disabled, out var error));
        Assert.Empty(disabled);
        Assert.Contains("kapalı", error, StringComparison.Ordinal);

        Assert.False(HybridClipboardPlan.TryCreate(
            "1", new[] { "MinimumWageSupportEnabled" }, 0, 0, 1,
            (_, _) => true, out var checkbox, out _));
        Assert.Empty(checkbox);
    }
}
