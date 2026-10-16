using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class MultiRouteProgressPaymentValidationServiceTests
{
    [Fact]
    public void FindSharedMonetaryInputs_ShouldAllowMultiRouteWhenAllSharedAmountsAreZero()
    {
        var fields = MultiRouteProgressPaymentValidationService.FindSharedMonetaryInputs(
            0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);

        Assert.Empty(fields);
    }

    [Fact]
    public void FindSharedMonetaryInputs_ShouldIdentifyEveryNonZeroSharedAmount()
    {
        var fields = MultiRouteProgressPaymentValidationService.FindSharedMonetaryInputs(
            discount: 1m,
            priceDifference: 2m,
            taxDebt: 3m,
            socialSecurityDebt: 4m,
            minimumWageSupport: 5m,
            inspectionPenalty: 6m,
            guideDiscount: 7m,
            guidePriceDifference: 8m);

        Assert.Equal(
            new[]
            {
                "Taşıma İskonto",
                "Taşıma Fiyat Farkı",
                "Rehber İskonto",
                "Rehber Fiyat Farkı",
                "Vergi Borcu",
                "SGK Borcu",
                "Asgari Ücret Desteği",
                "Denetim Cezası"
            },
            fields);
    }

    [Fact]
    public void FindSharedMonetaryInputs_ShouldIgnoreZeroAndPreserveNegativeValuesForReview()
    {
        var fields = MultiRouteProgressPaymentValidationService.FindSharedMonetaryInputs(
            discount: 0m,
            priceDifference: -1m,
            taxDebt: 0m,
            socialSecurityDebt: 0m,
            minimumWageSupport: 0m,
            inspectionPenalty: 0m,
            guideDiscount: 0m,
            guidePriceDifference: 0m);

        Assert.Equal(new[] { "Taşıma Fiyat Farkı" }, fields);
    }
}
