using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class RouteIdentityIsolationTests
{
    [Fact]
    public void SameContractorAndRouteNumber_ShouldRemainIndependentAcrossTenderNumbers()
    {
        var a = OehsIdentityService.BuildRouteKey("IH-A", "01", "123.456.789-01");
        var b = OehsIdentityService.BuildRouteKey("IH-B", "01", "12345678901");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void SameTenderAndRouteNumber_ShouldRemainIndependentAcrossContractors()
    {
        var a = OehsIdentityService.BuildRouteKey("IH-A", "01", "12345678901");
        var b = OehsIdentityService.BuildRouteKey("IH-A", "01", "98765432109");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ProgressPaymentKey_ShouldIncludeTenderRouteAndContractorIdentity()
    {
        var a = OehsIdentityService.BuildProgressPaymentKey(
            "IH-A", "01", "123.456.789-01", 1, new DateOnly(2026, 10, 31));

        var b = OehsIdentityService.BuildProgressPaymentKey(
            "IH-B", "01", "12345678901", 1, new DateOnly(2026, 10, 31));

        Assert.NotEqual(a, b);
        Assert.Contains("IH-A|01|12345678901|1|2026-10-31", a, StringComparison.Ordinal);
    }
}
