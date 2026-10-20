using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class RouteDraftCacheTests
{
    [Fact]
    public void SelectionChange_ShouldRetainDeselectedDraft_AndRestoreOnlyMatchingRoute()
    {
        var cache = new RouteDraftCache<Draft>();
        var routeA = new Draft { RouteKey = "A", PriceDifference = 100m };
        var routeB = new Draft { RouteKey = "B", PriceDifference = 200m };
        cache[routeA.RouteKey] = routeA;
        cache[routeB.RouteKey] = routeB;

        // A/B -> A/C: B remains cached but is not part of the selected/output set.
        var routeC = new Draft { RouteKey = "C", PriceDifference = 0m };
        cache[routeC.RouteKey] = routeC;
        var selectedAC = new[] { "A", "C" }.Select(key => cache[key]).ToArray();

        Assert.Equal(2, selectedAC.Length);
        Assert.Contains(routeA, selectedAC);
        Assert.Contains(routeC, selectedAC);
        Assert.DoesNotContain(routeB, selectedAC);
        Assert.Equal(200m, cache["B"].PriceDifference);

        // Restoring B reuses its own draft; no sibling's values are copied.
        var selectedAB = new[] { "A", "B" }.Select(key => cache[key]).ToArray();
        Assert.Contains(routeB, selectedAB);
        Assert.Equal(200m, selectedAB.Single(x => x.RouteKey == "B").PriceDifference);
        Assert.Equal(100m, selectedAB.Single(x => x.RouteKey == "A").PriceDifference);
    }

    [Fact]
    public void PeriodChange_ShouldResetEveryCachedDraft_IncludingDeselectedRoutes()
    {
        var cache = new RouteDraftCache<Draft>();
        cache["A"] = new Draft { RouteKey = "A", WorkDays = 18, PriceDifference = 100m };
        cache["B"] = new Draft { RouteKey = "B", WorkDays = 21, PriceDifference = 200m };
        cache["C"] = new Draft { RouteKey = "C", WorkDays = 0, PriceDifference = 0m };

        // Simulate the visible grid containing only A/C while B is deselected.
        var visibleRows = new[] { cache["A"], cache["C"] };
        Assert.DoesNotContain(cache["B"], visibleRows);

        cache.ResetAll(draft =>
        {
            draft.WorkDays = 0;
            draft.PriceDifference = 0m;
        });

        Assert.All(cache.Values, draft =>
        {
            Assert.Equal(0, draft.WorkDays);
            Assert.Equal(0m, draft.PriceDifference);
        });

        // Selecting B again for the new period must not restore last period's inputs.
        Assert.Equal(0, cache["B"].WorkDays);
        Assert.Equal(0m, cache["B"].PriceDifference);
    }

    [Fact]
    public void OutputSelection_ShouldExcludeCachedButDeselectedRoutes()
    {
        var cache = new RouteDraftCache<Draft>
        {
            ["A"] = new Draft { RouteKey = "A", PriceDifference = 10m },
            ["B"] = new Draft { RouteKey = "B", PriceDifference = 20m },
            ["C"] = new Draft { RouteKey = "C", PriceDifference = 30m }
        };

        var selected = SelectedRouteDrafts.SelectExact(new[] { "A", "C" }, cache);

        Assert.Equal(new[] { "A", "C" }, selected.Select(x => x.RouteKey));
        Assert.DoesNotContain(selected, x => x.RouteKey == "B");
        Assert.Equal(40m, selected.Sum(x => x.PriceDifference));
    }

    [Fact]
    public void OutputSelection_ShouldFailClosedWhenSelectedRouteHasNoDraft()
    {
        var cache = new RouteDraftCache<Draft>
        {
            ["A"] = new Draft { RouteKey = "A" }
        };

        Assert.Throws<InvalidOperationException>(() =>
            SelectedRouteDrafts.SelectExact(new[] { "A", "B" }, cache));
    }

    [Fact]
    public void OutputSelection_ShouldRejectDuplicateSelectedRouteKeys()
    {
        var cache = new RouteDraftCache<Draft>
        {
            ["A"] = new Draft { RouteKey = "A" }
        };

        Assert.Throws<InvalidOperationException>(() =>
            SelectedRouteDrafts.SelectExact(new[] { "A", "A" }, cache));
    }

    private sealed class Draft
    {
        public string RouteKey { get; init; } = string.Empty;
        public int WorkDays { get; set; }
        public decimal PriceDifference { get; set; }
    }
}
