using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class MonthlyProgressPaymentIdentityServiceTests
{
    [Fact]
    public void BuildDisplayName_ShouldIncludeProgramTenderContractorRoutesAndMonth()
    {
        var name = MonthlyProgressPaymentIdentityService.BuildDisplayName(
            TransportationProgramType.SpecialEducation,
            "2026-454",
            "S-2026",
            "A Firması",
            new[] { "Q3", "Q1", "Q2" },
            new DateOnly(2026, 10, 31));

        Assert.Equal("Özel Eğitim | İKN 2026-454 • Sözleşme S-2026 | A Firması | Q1+Q2+Q3 | Ekim 2026", name);
    }

    [Fact]
    public void SuggestNextRouteNumber_ShouldCountPriorPositiveMonthsPerProgramAndRoute()
    {
        var history = new[]
        {
            Payment("Q1", TransportationProgramType.SpecialEducation, new DateOnly(2026, 9, 30)),
            Payment("Q1", TransportationProgramType.SpecialEducation, new DateOnly(2026, 12, 31)),
            Payment("Q1", TransportationProgramType.BasicOrSecondary, new DateOnly(2026, 9, 30)),
            Payment("Q3", TransportationProgramType.SpecialEducation, new DateOnly(2026, 10, 31))
        };

        var q1Number = MonthlyProgressPaymentIdentityService.SuggestNextRouteNumber(
            history, "2026-454", "S-2026", "ID-1", "Q1",
            TransportationProgramType.SpecialEducation, new DateOnly(2026, 10, 31));
        var q3Number = MonthlyProgressPaymentIdentityService.SuggestNextRouteNumber(
            history, "2026-454", "S-2026", "ID-1", "Q3",
            TransportationProgramType.SpecialEducation, new DateOnly(2026, 10, 31));

        Assert.Equal(2, q1Number);
        Assert.Equal(1, q3Number);
    }

    [Fact]
    public void SuggestNextRouteNumber_ShouldNotCountZeroOrNegativeMonthlyAmounts()
    {
        var zero = Payment("Q1", TransportationProgramType.SpecialEducation, new DateOnly(2026, 9, 30), workDays: 0);
        var negative = Payment(
            "Q1", TransportationProgramType.SpecialEducation, new DateOnly(2026, 8, 31),
            workDays: 1, dailyPrice: 100m, priceDifference: -150m);
        var history = new[] { zero, negative };

        var number = MonthlyProgressPaymentIdentityService.SuggestNextRouteNumber(
            history, "2026-454", "S-2026", "ID-1", "Q1",
            TransportationProgramType.SpecialEducation, new DateOnly(2026, 10, 31));

        Assert.Equal(1, number);
    }

    private static ProgressPaymentHistoryData Payment(
        string route,
        TransportationProgramType program,
        DateOnly period,
        int workDays = 1,
        decimal dailyPrice = 100m,
        decimal priceDifference = 0m)
    {
        var input = new ProgressPaymentInput(
            workDays, dailyPrice, 0m, priceDifference, 1000m, true,
            0m, 0m, 0m, ProgramType: program);
        return new ProgressPaymentHistoryData(
            Guid.NewGuid(),
            period.ToDateTime(TimeOnly.MinValue),
            input,
            ProgressPaymentCalculator.Calculate(input),
            ProgressPaymentNumber: 1,
            PeriodDate: period,
            RouteNumber: route,
            RouteName: $"Hat {route}",
            WorkName: "İş",
            ContractorName: "A Firması",
            TenderNumber: "2026-454",
            ContractNumber: "S-2026",
            ContractorTaxOrIdentityNumber: "ID-1");
    }

    [Fact]
    public void SuggestNextRouteNumber_ShouldIgnoreHistoryWithMissingContractorIdentity()
    {
        var previous = Payment("Q1", TransportationProgramType.SpecialEducation, new DateOnly(2026, 9, 30))
            with { ContractorTaxOrIdentityNumber = "" };

        var number = MonthlyProgressPaymentIdentityService.SuggestNextRouteNumber(
            new[] { previous }, "2026-454", "S-2026", "", "Q1",
            TransportationProgramType.SpecialEducation, new DateOnly(2026, 10, 31));

        Assert.Equal(1, number);
    }

    [Fact]
    public void SuggestNextRouteNumber_ShouldIgnoreIdentityThatNormalizesToEmpty()
    {
        var previous = Payment("Q1", TransportationProgramType.SpecialEducation, new DateOnly(2026, 9, 30))
            with { ContractorTaxOrIdentityNumber = "----" };

        var number = MonthlyProgressPaymentIdentityService.SuggestNextRouteNumber(
            new[] { previous }, "2026-454", "S-2026", "----", "Q1",
            TransportationProgramType.SpecialEducation, new DateOnly(2026, 10, 31));

        Assert.Equal(1, number);
    }
}
