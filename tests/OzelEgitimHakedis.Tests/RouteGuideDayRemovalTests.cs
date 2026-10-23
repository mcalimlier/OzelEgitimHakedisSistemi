using System.Text.Json;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class RouteGuideDayRemovalTests
{
    [Fact]
    public void LegacyRouteAnnualGuideDays_ShouldBeIgnoredAndNeverWrittenBack()
    {
        const string legacy = """
            {"RouteNumber":"1","Name":"Hat","DailyPrice":1000,"GuideWorkDays":180,
             "GuideDailyPrice":500,"TransportWorkDays":20,"TenderNumber":"T","ContractNumber":"C",
             "ContractorTaxOrIdentityNumber":"TEST-FIRM"}
            """;
        var route = JsonSerializer.Deserialize<RouteData>(legacy)!;
        var rewritten = JsonSerializer.Serialize(route);
        Assert.DoesNotContain("GuideWorkDays", rewritten, StringComparison.Ordinal);
        Assert.Equal(500m, route.GuideDailyPrice);
        Assert.Equal(20m, route.TransportWorkDays);
    }

    [Fact]
    public void RoutePriceEdit_ShouldKeepThePaymentsMonthlyGuideDays()
    {
        var route = JsonSerializer.Deserialize<RouteData>("""
            {"RouteNumber":"1","Name":"Hat","DailyPrice":1000,"GuideWorkDays":180,
             "GuideDailyPrice":500,"TransportWorkDays":20,"TenderNumber":"T","ContractNumber":"C",
             "ContractorTaxOrIdentityNumber":"TEST-FIRM"}
            """)!;
        var input = new ProgressPaymentInput(20,1000,0,0,100000,false,0,0,0,GuideWorkDays:18,GuideDailyPrice:500);
        var payment = new ProgressPaymentHistoryData(Guid.NewGuid(),DateTime.UtcNow,input,
            ProgressPaymentCalculator.Calculate(input),1,new DateOnly(2026,10,31),"1","Hat",TenderNumber:"T",
            ContractNumber:"C",ContractorTaxOrIdentityNumber:"TEST-FIRM");
        var updated = Assert.Single(ProgressPaymentPropagationService.RecalculateForRouteChange(
            new[]{payment},route,route with {GuideDailyPrice=600},new Contractor("Test","TEST-FIRM")));
        Assert.Equal(18m,updated.Input.GuideWorkDays);
        Assert.Equal(10800m,updated.Calculation.GuideServiceAmount);
    }
}
