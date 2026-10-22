using Xunit;
using OzelEgitimHakedis.Wpf;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class WindowLayoutPreferenceTests
{
    [Fact]
    public void DefaultPreference_ShouldStartMaximized()
    {
        Assert.Equal(WindowLayoutMode.Maximized, WindowLayoutPreference.Default.Mode);
    }

    [Theory]
    [InlineData(WindowLayoutMode.FullHd, 1920, 1080)]
    [InlineData(WindowLayoutMode.Hd, 1366, 768)]
    [InlineData(WindowLayoutMode.StandardFourByThree, 1280, 1024)]
    [InlineData(WindowLayoutMode.StandardWide, 1280, 768)]
    public void FixedPreset_ShouldExposeItsRequestedDimensions(
        WindowLayoutMode mode,
        double expectedWidth,
        double expectedHeight)
    {
        var preference = new WindowLayoutPreference(mode);

        var size = preference.GetRequestedSize();

        Assert.NotNull(size);
        Assert.Equal(expectedWidth, size!.Value.Width);
        Assert.Equal(expectedHeight, size.Value.Height);
    }

    [Fact]
    public void MaximizedPreset_ShouldNotRequestAFixedWindowSize()
    {
        var size = WindowLayoutPreference.Default.GetRequestedSize();

        Assert.Null(size);
    }

    [Theory]
    [InlineData(1920, 215)]
    [InlineData(1366, 215)]
    [InlineData(1280, 185)]
    [InlineData(1024, 185)]
    public void NavigationWidth_ShouldCompactOnlyAtStandardAndSmallerWidths(
        double windowWidth,
        double expectedNavigationWidth)
    {
        Assert.Equal(expectedNavigationWidth, WindowLayoutMetrics.GetNavigationWidth(windowWidth));
    }
}
