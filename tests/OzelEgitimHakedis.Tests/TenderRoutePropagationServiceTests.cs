using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class TenderRoutePropagationServiceTests
{
    [Fact]
    public void ChangeTenderNumber_ShouldPreserveRoutePricingAndGuideValues()
    {
        var routes = new[]
        {
            new RouteData(
                "1", "Hat 1", 2608.26m, 1362.49m, 29,
                "OLD", "12345678901")
        };

        var updated = TenderRoutePropagationService.UpdateTenderNumber(
            routes, "OLD", "NEW");

        var route = Assert.Single(updated);
        Assert.Equal("NEW", route.TenderNumber);
        Assert.Equal(2608.26m, route.DailyPrice);
        Assert.Equal(29, route.TransportWorkDays);
        Assert.Equal(1362.49m, route.GuideDailyPrice);
    }
}
