using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class PaymentOperationsTests
{
    internal static ContractDraftData Contract => new("2026/42", "INTERNAL-a", "", "", 100000m,
        new DateOnly(2026, 9, 1), new DateOnly(2027, 6, 30), false);
    internal static RouteData Route => new("1", "Işık İlkokulu", 19539.1608m,
        TenderNumber: Contract.TenderNumber, ContractNumber: Contract.ContractNumber, ContractorTaxOrIdentityNumber: "123");
    internal static ProgressPaymentHistoryData Payment => new(Guid.NewGuid(), DateTime.Now,
        new ProgressPaymentInput(0, 19539.1608m, 0, 100, 100000, true, 0, 0, 0),
        ProgressPaymentCalculator.Calculate(new ProgressPaymentInput(0, 19539.1608m, 0, 100, 100000, true, 0, 0, 0)),
        1, new DateOnly(2026, 10, 1), "1", Route.Name, ContractorName: "IŞIK TAŞIMA",
        TenderNumber: Contract.TenderNumber, ContractNumber: Contract.ContractNumber, ContractorTaxOrIdentityNumber: "123",
        ContractStartDate: Contract.StartDate, ContractEndDate: Contract.EndDate);
    internal static OehsApplicationData Data(ProgressPaymentHistoryData? payment = null) => new(Contract,
        new[] { Route }, new[] { new Contractor("IŞIK TAŞIMA", "123") },
        payment is null ? Array.Empty<ProgressPaymentHistoryData>() : new[] { payment }, new[] { Contract });

    [Fact] public void MonthlyTracking_KeepsMissingPaymentsAndZeroDayPriceDifferences()
    {
        var missing = Assert.Single(PaymentOperationsService.BuildMonth(Data(), Contract.ProgramType, new DateOnly(2026,10,1)));
        Assert.Equal("Hakediş yok", missing.Status);
        var ready = Assert.Single(PaymentOperationsService.BuildMonth(Data(Payment), Contract.ProgramType, new DateOnly(2026,10,1)));
        Assert.Equal("Kontrole hazır", ready.Status);
        Assert.Empty(ready.Issues);
    }
    [Fact] public void MonthlyTracking_SeparatesProgramsPeriodsAndDetectsDuplicatePayments()
    {
        Assert.Empty(PaymentOperationsService.BuildMonth(Data(Payment), TransportationProgramType.BasicOrSecondary, new DateOnly(2026,10,1)));
        Assert.Empty(PaymentOperationsService.BuildMonth(Data(), Contract.ProgramType, new DateOnly(2027,7,1)));
        var p = Payment;
        var duplicate = Data(p) with { ProgressPayments = new[] { p, p with { Id = Guid.NewGuid() } } };
        Assert.Contains(Assert.Single(PaymentOperationsService.BuildMonth(duplicate, Contract.ProgramType, new DateOnly(2026,10,1))).Issues,
            x => x.Message.Contains("Birden fazla", StringComparison.Ordinal));
    }
    [Fact] public void Search_UsesTurkishCaseAndPaymentPeriod()
    {
        var p = Payment;
        Assert.True(PaymentOperationsService.Matches(p, new PaymentFilter(Search: "ışık", Month: new DateOnly(2026,10,20))));
        Assert.False(PaymentOperationsService.Matches(p, new PaymentFilter(Month: new DateOnly(2026,9,1))));
        Assert.False(PaymentOperationsService.Matches(p, new PaymentFilter(UndatedOnly: true)));
    }
    [Fact] public void Differences_PreserveDecimalsAndIncludeIdentityOnlyChanges()
    {
        var p = Payment;
        var changed = p with { Input = p.Input with { DailyPrice = 19539.1609m }, ContractorName = "Yeni firma" };
        var differences = PaymentOperationsService.Compare(p, changed);
        Assert.Contains(differences, x => x.Field == "Taşıma günlük fiyatı" && x.Before.Contains("1608", StringComparison.Ordinal));
        Assert.Contains(differences, x => x.Field == "Yüklenici");
        Assert.Empty(PaymentOperationsService.Compare(p, p));
    }
    [Fact] public void Preflight_RequiresIknAndDatesButNotOfficialContractNumber()
    {
        Assert.Empty(PaymentOperationsService.Validate(Payment, Contract, Route, Data().Contractors[0]));
        Assert.Contains(PaymentOperationsService.Validate(Payment with { TenderNumber = "" }, Contract, Route, Data().Contractors[0]),
            x => x.Target == "Contract");
    }
    [Fact] public void MonthlyTracking_DoesNotHideOrphanRoutes()
    {
        var data = Data() with { Contracts = Array.Empty<ContractDraftData>(), Contract = null };
        Assert.NotEmpty(Assert.Single(PaymentOperationsService.BuildMonth(data, Contract.ProgramType, new DateOnly(2026,10,1))).Issues);
    }
}
