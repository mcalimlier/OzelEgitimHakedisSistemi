using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentPropagationServiceTests
{
    [Fact]
    public void ContractChange_ShouldUpdateTransportDailyPriceAndRecalculate()
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

        Assert.Equal(1250m, result.Input.DailyPrice);
        Assert.Equal(12500m, result.Calculation.ServiceAmount);
        Assert.Equal(newContract.ContractAmount, result.Input.ContractAmount);
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
        Assert.Equal(12500m, result.Calculation.ServiceAmount);
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
