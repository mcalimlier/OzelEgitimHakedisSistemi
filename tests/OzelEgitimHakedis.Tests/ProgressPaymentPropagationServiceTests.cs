using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentPropagationServiceTests
{
    [Fact]
    public void ContractChange_ShouldNotReplacePaymentRoutePriceAmountOrDate()
    {
        var oldContract = CreateContract(1000m);
        var newContract = oldContract with
        {
            TransportDailyPrice = 1250m,
            ContractAmount = 250000m,
            ContractDate = new DateOnly(2026, 2, 1)
        };
        var payment = CreatePayment();

        var result = Assert.Single(ProgressPaymentPropagationService.RecalculateForContractChange(
            new[] { payment }, Array.Empty<RouteData>(), oldContract, newContract));

        Assert.Equal(1000m, result.Input.DailyPrice);
        Assert.Equal(100000m, result.Input.ContractAmount);
        Assert.Equal(new DateOnly(2025, 12, 15), result.ContractDate);
    }

    [Fact]
    public void RouteMetadataChange_ShouldPreservePeriodDaysAndPrices()
    {
        var original = new RouteData("01", "Hat 01", 1000m, GuideDailyPrice: 100m, TransportWorkDays: 20,
            TenderNumber: "IHALE-01", ContractorTaxOrIdentityNumber: "12345678901", ContractNumber: "SOZ-01", ContractAmount: 100000m);
        var payment = CreatePayment();
        var input = payment.Input with { WorkDays = 9, DailyPrice = 1100m, GuideDailyPrice = 125m };
        payment = payment with { Input = input, Calculation = ProgressPaymentCalculator.Calculate(input) };
        var changed = original with { ContractAmount = 120000m, ContractDate = new DateOnly(2026, 1, 4) };
        var result = Assert.Single(ProgressPaymentPropagationService.RecalculateForRouteChange(
            new[] { payment }, original, changed, new Contractor("Firma", "12345678901")));
        Assert.Equal(9m,result.Input.WorkDays);
        Assert.Equal(1100m,result.Input.DailyPrice);
        Assert.Equal(125m,result.Input.GuideDailyPrice);
        Assert.Equal(120000m,result.Input.ContractAmount);
    }

    [Fact]
    public void RouteChange_ShouldUseItsOwnPriceAmountAndDate()
    {
        var oldRoute = new RouteData("01", "Hat 01", 1000m, TransportWorkDays: 10,
            TenderNumber: "IHALE-01", ContractorTaxOrIdentityNumber: "12345678901", ContractNumber: "SOZ-01");
        var updatedRoute = oldRoute with { DailyPrice = 1234.56m, ContractAmount = 76543.21m, ContractDate = new DateOnly(2026, 1, 3) };
        var result = Assert.Single(ProgressPaymentPropagationService.RecalculateForRouteChange(
            new[] { CreatePayment() }, oldRoute, updatedRoute, new Contractor("Firma", "12345678901")));
        Assert.Equal(1234.56m, result.Input.DailyPrice);
        Assert.Equal(76543.21m, result.Input.ContractAmount);
        Assert.Equal(new DateOnly(2026, 1, 3), result.ContractDate);
        Assert.Equal(12345.60m, result.Calculation.ServiceAmount);
    }

    [Fact]
    public void RouteChange_ShouldNotOverwriteOtherRouteInSameTender()
    {
        foreach (var program in new[] { TransportationProgramType.SpecialEducation, TransportationProgramType.BasicOrSecondary })
        {
        var first = CreatePayment();
        var firstInput = first.Input with { ProgramType = program };
        first = first with { Input = firstInput, Calculation = ProgressPaymentCalculator.Calculate(firstInput) };
        var secondInput = firstInput with { WorkDays = 12, DailyPrice = 2200m, ContractAmount = 200000m };
        var second = first with { Id = Guid.NewGuid(), RouteNumber = "02", Input = secondInput,
            Calculation = ProgressPaymentCalculator.Calculate(secondInput), ContractDate = new DateOnly(2025, 11, 20) };
        var original = new RouteData("01", "Hat 01", 1000m, TransportWorkDays: 10,
            TenderNumber: "IHALE-01", ContractorTaxOrIdentityNumber: "12345678901", ContractNumber: "SOZ-01", ProgramType: program);
        var changed = original with { TransportWorkDays = 8, DailyPrice = 1300m, ContractAmount = 150000m, ContractDate = new DateOnly(2025, 12, 20) };

        var result = ProgressPaymentPropagationService.RecalculateForRouteChange(
            new[] { first, second }, original, changed, new Contractor("Firma", "12345678901", ProgramType: program));

        Assert.Equal(8, result[0].Input.WorkDays);
        Assert.Equal(1300m, result[0].Input.DailyPrice);
        Assert.Equal(150000m, result[0].Input.ContractAmount);
        Assert.Equal(new DateOnly(2025, 12, 20), result[0].ContractDate);
        Assert.Equal(12, result[1].Input.WorkDays);
        Assert.Equal(2200m, result[1].Input.DailyPrice);
        Assert.Equal(200000m, result[1].Input.ContractAmount);
        Assert.Equal(new DateOnly(2025, 11, 20), result[1].ContractDate);
        }
    }

    [Fact]
    public void ContractChange_ShouldPreserveRouteDailyPriceAndUpdateIdentity()
    {
        var oldContract = CreateContract(1000m);
        var newContract = CreateContract(1250m);
        var payment = CreatePayment();

        var updated = ProgressPaymentPropagationService.RecalculateForContractChange(
            new[] { payment },
            Array.Empty<RouteData>(),
            oldContract,
            newContract);

        var result = Assert.Single(updated);

        Assert.Equal(1000m, result.Input.DailyPrice);
        Assert.Equal(10000m, result.Calculation.ServiceAmount);
        Assert.Equal(payment.Input.ContractAmount, result.Input.ContractAmount);
        Assert.Equal(newContract.WorkName, result.WorkName);
        Assert.Equal(newContract.TenderNumber, result.TenderNumber);
        Assert.Equal(newContract.ContractNumber, result.ContractNumber);
    }

    [Fact]
    public void ContractChange_ShouldPreserveVatExemptionCapturedOnIssuedPayment()
    {
        var oldContract = CreateContract(1000m) with { VatExempt = false };
        var newContract = CreateContract(1250m) with { VatExempt = true };
        var payment = CreatePayment();
        var exemptInput = payment.Input with { IsVatExempt = true };
        payment = payment with
        {
            Input = exemptInput,
            Calculation = ProgressPaymentCalculator.Calculate(exemptInput)
        };

        var updated = ProgressPaymentPropagationService.RecalculateForContractChange(
            new[] { payment },
            Array.Empty<RouteData>(),
            oldContract,
            newContract);

        var result = Assert.Single(updated);

        Assert.True(result.Input.IsVatExempt);
        Assert.Equal(0m, result.Calculation.TransportVat);
        Assert.Equal(10000m, result.Calculation.ServiceAmount);
    }

    private static ContractDraftData CreateContract(decimal transportDailyPrice) =>
        new(
            "IHALE-01",
            "SOZ-01",
            "Özel Eğitim Taşıma",
            "Firma",
            100000m,
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            false,
            TenderDate: new DateOnly(2025, 12, 1),
            ContractDate: new DateOnly(2025, 12, 15),
            ContractorTaxOrIdentityNumber: "12345678901",
            ContractWorkDays: 200,
            TransportDailyPrice: transportDailyPrice);

    private static ProgressPaymentHistoryData CreatePayment() =>
        new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new ProgressPaymentInput(
                10,
                1000m,
                0m,
                0m,
                100000m,
                false,
                0m,
                0m,
                0m),
            ProgressPaymentCalculator.Calculate(
                new ProgressPaymentInput(
                    10,
                    1000m,
                    0m,
                    0m,
                    100000m,
                    false,
                    0m,
                    0m,
                    0m)),
            1,
            new DateOnly(2026, 10, 31),
            "01",
            "Hat 01",
            "Özel Eğitim Taşıma",
            "Firma",
            "IHALE-01",
            "SOZ-01",
            new DateOnly(2025, 12, 1),
            new DateOnly(2025, 12, 15),
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 12, 31),
            "",
            "12345678901");
}
