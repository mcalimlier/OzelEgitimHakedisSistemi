using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Latest uploaded Basic/Secondary XLSM source rows 4-8.
/// SHA-256: 37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296.
/// Cached-cell regression only; not desktop Excel/VBA execution.
/// </summary>
public sealed class CurrentBasicWorkbookParityTests
{
    [Fact]
    public void LatestBasicRow4_ShouldPreserveFullPrecisionUntilOfficialReportRounding()
    {
        var input = new ProgressPaymentInput(16m, 19539.1608m, 0m, 7365.35m, 100000m,
            false, 0m, 0m, 0m, ProgramType: TransportationProgramType.BasicOrSecondary);
        var payment = new ProgressPaymentHistoryData(Guid.NewGuid(), DateTime.UtcNow,
            input, ProgressPaymentCalculator.Calculate(input), 1, new DateOnly(2026, 10, 31));
        var totals = ProgressPaymentReportService.Calculate(payment, new[] { payment });

        Assert.Equal(312626.5728m, totals.CumulativeServiceAmount);
        Assert.Equal(332516.074511856m, payment.Calculation.NetPayable);
        // The report rounds gross (351991.12) and deductions (19475.04) separately.
        Assert.Equal(332516.08m, ProgressPaymentReportService.CalculateReportNetPayable(payment, totals));
    }

    [Fact]
    public void LatestBasicRows4To8_ShouldMatchWorkbookCalculationCells()
    {
        var rows = new[]
        {
            (Input: new ProgressPaymentInput(16m, 19539.1608m, 0m, 7365.35m, 100000m, false, 0m, 0m, 0m, ProgramType: TransportationProgramType.BasicOrSecondary),
                TransportNet: 319991.9228m, Vat: 31999.19228m, Withholding: 15999.59614m, Gross: 351991.11508m, Guarantee: 441.921m, StampTax: 3033.523428144m, Deductions: 19475.040568144m, Net: 332516.074511856m),
            (Input: new ProgressPaymentInput(16m, 6545m, 0m, 7366.35m, 100000m, false, 0m, 0m, 0m, ProgramType: TransportationProgramType.BasicOrSecondary),
                TransportNet: 112086.35m, Vat: 11208.635m, Withholding: 5604.3175m, Gross: 123294.985m, Guarantee: 441.981m, StampTax: 1062.578598m, Deductions: 7108.877098m, Net: 116186.107902m),
            (Input: new ProgressPaymentInput(16m, 6135m, 0m, 7367.35m, 100000m, false, 0m, 0m, 0m, ProgramType: TransportationProgramType.BasicOrSecondary),
                TransportNet: 105527.35m, Vat: 10552.735m, Withholding: 5276.3675m, Gross: 116080.085m, Guarantee: 442.041m, StampTax: 1000.399278m, Deductions: 6718.807778m, Net: 109361.277222m),
            (Input: new ProgressPaymentInput(16m, 4857.1601m, 0m, 7368.35m, 100000m, false, 0m, 0m, 0m, ProgramType: TransportationProgramType.BasicOrSecondary),
                TransportNet: 85082.9116m, Vat: 8508.29116m, Withholding: 4254.14558m, Gross: 93591.20276m, Guarantee: 442.101m, StampTax: 806.586001968m, Deductions: 5502.832581968m, Net: 88088.370178032m),
            (Input: new ProgressPaymentInput(16m, 2810m, 0m, 7369.35m, 100000m, false, 0m, 0m, 0m, ProgramType: TransportationProgramType.BasicOrSecondary),
                TransportNet: 52329.35m, Vat: 5232.935m, Withholding: 2616.4675m, Gross: 57562.285m, Guarantee: 442.161m, StampTax: 496.082238m, Deductions: 3554.710738m, Net: 54007.574262m)
        };

        foreach (var row in rows)
        {
            var result = ProgressPaymentCalculator.Calculate(row.Input);
            Assert.Equal(row.TransportNet, result.TransportNetAmount);
            Assert.Equal(row.Vat, result.Vat);
            Assert.Equal(row.Withholding, result.Withholding);
            Assert.Equal(row.Gross, result.GrossAmount);
            Assert.Equal(row.Guarantee, result.AdditionalGuarantee);
            Assert.Equal(row.StampTax, result.StampTax);
            Assert.Equal(row.Deductions, result.TotalDeductions);
            Assert.Equal(row.Net, result.NetPayable);
        }
    }
}
