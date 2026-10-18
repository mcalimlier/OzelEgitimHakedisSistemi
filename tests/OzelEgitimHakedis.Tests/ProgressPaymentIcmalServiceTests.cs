using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentIcmalServiceTests
{
    [Fact]
    public void BuildPeriods_ShouldMatchXlsmElevenMonthCycle()
    {
        var periods = ProgressPaymentIcmalService.BuildPeriods(new DateOnly(2026, 10, 31));

        Assert.Equal(11, periods.Count);
        Assert.Equal(new DateOnly(2026, 9, 1), periods[0]);
        Assert.Equal(new DateOnly(2027, 7, 1), periods[^1]);
    }

    [Fact]
    public void BuildPeriods_ShouldPreserveFullSeptemberThroughJulyCalendarOrder()
    {
        var periods = ProgressPaymentIcmalService.BuildPeriods(new DateOnly(2026, 11, 30));

        var expected = Enumerable.Range(0, 11)
            .Select(index => new DateOnly(2026, 9, 1).AddMonths(index))
            .ToArray();

        Assert.Equal(expected, periods);
        Assert.Equal(new DateOnly(2026, 11, 1), periods[2]);
        Assert.Equal(new DateOnly(2026, 12, 1), periods[3]);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldUseImmediatePreviousPaymentOnly()
    {
        var older = History(1, new DateOnly(2026, 9, 30), 100m);
        var immediatePrevious = History(2, new DateOnly(2026, 10, 31), 200m);
        var current = History(3, new DateOnly(2026, 11, 30), 300m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { older, immediatePrevious, current }, current);

        Assert.Equal(200m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldCarryBasicProgramDespiteSourceLookupRangeDefect()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            Input = new ProgressPaymentInput(
                1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            Input = new ProgressPaymentInput(
                1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current);

        Assert.Equal(200m, result);
    }

    [Fact]
    public void BasicE21_November2026_ShouldCarryOctoberDeductionFromSourceExample()
    {
        // Source workbook: Temel Eğitim, November 2026. The previous-month
        // deduction is İcmal Verileri!AC4 = 24,104.02472 TL (October), in AB:AL.
        // E4 is the selected period; E5 is the payment date and must not drive this lookup.
        var october = History(1, new DateOnly(2026, 10, 31), 24104.02472m) with
        {
            Input = new ProgressPaymentInput(
                13, 6545m, 0m, 3659.2m, 100000m, true, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };
        var november = History(2, new DateOnly(2026, 11, 30), 1060.847016m) with
        {
            Input = new ProgressPaymentInput(
                13, 6545m, 0m, 3659.2m, 100000m, true, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { october, november }, november);

        Assert.Equal(24104.02472m, result);
    }

    [Fact]
    public void SpecialEducationE21_November2026_ShouldCarryOctoberDeductionFromSourceExample()
    {
        // Source workbook: Özel Eğitim, November 2026. Hakediş İcmali!E21
        // has cached result 10,580.582128 TL, matching İcmal Verileri!AO4
        // (October 2026). Special Education uses E5 and AN:AX, unlike Basic.
        var october = History(1, new DateOnly(2026, 10, 31), 10580.582128m);
        var november = History(2, new DateOnly(2026, 11, 30), 11201.1393472m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { october, november }, november);

        Assert.Equal(10580.582128m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryAcrossTransportationPrograms()
    {
        var basicPrevious = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            Input = new ProgressPaymentInput(
                1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { basicPrevious, current }, current);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldReturnZeroWhenImmediatelyPreviousMonthHasNoPayment()
    {
        var september = History(1, new DateOnly(2026, 9, 30), 100m);
        var november = History(2, new DateOnly(2026, 11, 30), 300m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { september, november }, november);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void MonthLookupRange_ShouldIncludeSeptemberThroughJuneButNotJulyPlaceholder()
    {
        Assert.True(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(new DateOnly(2026, 9, 1)));
        Assert.True(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(new DateOnly(2027, 6, 1)));
        Assert.False(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(new DateOnly(2027, 7, 1)));
        Assert.False(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(new DateOnly(2027, 8, 1)));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldCarryMayDeductionIntoJune()
    {
        var may = History(1, new DateOnly(2027, 5, 31), 185m);
        var june = History(2, new DateOnly(2027, 6, 30), 240m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { may, june }, june);

        Assert.Equal(185m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldReturnZeroForFirstSeptemberOfFiscalYear()
    {
        var august = History(1, new DateOnly(2026, 8, 31), 450m);
        var september = History(2, new DateOnly(2026, 9, 30), 250m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { august, september }, september);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldUseNovemberDespiteMissingSourceHeader()
    {
        var november = History(1, new DateOnly(2026, 11, 30), 180m);
        var december = History(2, new DateOnly(2026, 12, 31), 250m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { november, december }, december);

        Assert.Equal(180m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldReturnZeroForJulyPlaceholderLookup()
    {
        var june = History(1, new DateOnly(2027, 6, 30), 180m);
        var july = History(2, new DateOnly(2027, 7, 31), 250m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { june, july }, july);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldCarryJuneDeductionIntoJulyForBasicProgram()
    {
        var june = History(1, new DateOnly(2027, 6, 30), 180m) with
        {
            Input = new ProgressPaymentInput(
                1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };
        var july = History(2, new DateOnly(2027, 7, 31), 250m) with
        {
            Input = new ProgressPaymentInput(
                1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { june, july }, july);

        Assert.Equal(180m, result);
    }

    [Fact]
    public void BuildPeriods_ShouldKeepSeptemberAsFiscalYearStart()
    {
        var periods = ProgressPaymentIcmalService.BuildPeriods(new DateOnly(2026, 9, 30));

        Assert.Equal(new DateOnly(2026, 9, 1), periods[0]);
        Assert.Equal(new DateOnly(2027, 7, 1), periods[^1]);
    }

    [Fact]
    public void BuildPeriods_ShouldTreatAugustAsTheEndOfPreviousFiscalCycle()
    {
        var periods = ProgressPaymentIcmalService.BuildPeriods(new DateOnly(2026, 8, 31));

        Assert.Equal(new DateOnly(2025, 9, 1), periods[0]);
        Assert.Equal(new DateOnly(2026, 7, 1), periods[^1]);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryAcrossRoutes()
    {
        var otherRoute = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            RouteNumber = "Q2"
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            RouteNumber = "Q1"
        };

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { otherRoute, current }, current);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldCrossCalendarYearFromJanuaryToDecember()
    {
        var previousDecember = History(1, new DateOnly(2026, 12, 31), 420m);
        var currentJanuary = History(2, new DateOnly(2027, 1, 31), 180m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previousDecember, currentJanuary }, currentJanuary);

        Assert.Equal(420m, result);
    }

    [Fact]
    public void MonthLookupRange_ShouldKeepJulyProgramSpecificAndExcludeAugust()
    {
        Assert.True(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(
            new DateOnly(2027, 7, 1), TransportationProgramType.BasicOrSecondary));
        Assert.False(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(
            new DateOnly(2027, 8, 1), TransportationProgramType.BasicOrSecondary));
        Assert.False(ProgressPaymentIcmalService.IsMonthInWorkbookLookupRange(
            new DateOnly(2027, 7, 1), TransportationProgramType.SpecialEducation));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNormalizeIdentityFieldsLikeOtherCoreServices()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            TenderNumber = " TENDER ",
            ContractNumber = " soz ",
            RouteNumber = " 1 ",
            ContractorTaxOrIdentityNumber = "123-456-789-01"
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            TenderNumber = "tender",
            ContractNumber = "SOZ",
            RouteNumber = "1",
            ContractorTaxOrIdentityNumber = "12345678901"
        };

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current);

        Assert.Equal(200m, result);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryAcrossTenderNumbers()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            TenderNumber = "OTHER-TENDER"
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            TenderNumber = "TENDER"
        };

        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryAcrossContractNumbers()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            ContractNumber = "OTHER-CONTRACT"
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            ContractNumber = "SOZ"
        };

        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryAcrossContractorIdentityNumbers()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            ContractorTaxOrIdentityNumber = "99999999999"
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            ContractorTaxOrIdentityNumber = "12345678901"
        };

        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryAcrossProgramEvenWhenOtherIdentityFieldsMatch()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            Input = new ProgressPaymentInput(
                1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary)
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m);

        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current));
    }

    private static ProgressPaymentHistoryData History(
        int number,
        DateOnly period,
        decimal deduction) =>
        new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new ProgressPaymentInput(1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m),
            new ProgressPaymentCalculation(
                1000m, 0m, 1000m, 0m, 0m, 1000m, 0m, 1000m, deduction, 1000m - deduction, 0m,
                new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m)),
            number,
            period,
            "1", "Hat 1", "İş", "Firma", "TENDER", "SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");
}
