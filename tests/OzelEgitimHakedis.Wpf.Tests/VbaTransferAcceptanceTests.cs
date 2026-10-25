using System.IO;
using System.Text.Json;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    [Fact]
    public void ActualVbaTransfers_ShouldPreserveCalculationsReportsAndPreviousDeductionAcrossFourteenPeriods()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllText(LoadProjectFile("tests", "OzelEgitimHakedis.Wpf.Tests", "Fixtures", "excel-vba-periods-2026-10-25.json")));
        var samples = fixture.RootElement.EnumerateArray().ToArray();
        Assert.Equal(14, samples.Length);
        var sources = ReadExcelAuditCases().Where(c => c.Scenario == "source" && c.Row == 4).ToArray();
        foreach (var programGroup in samples.GroupBy(s => s.GetProperty("Program").GetString()))
        {
            var program = Enum.Parse<TransportationProgramType>(programGroup.Key!);
            var history = new List<ProgressPaymentHistoryData>();
            foreach (var sample in programGroup.OrderBy(s => s.GetProperty("Offset").GetInt32()))
            {
                var offset = sample.GetProperty("Offset").GetInt32();
                var input = sources.Single(s => s.Input.ProgramType == program).Input with
                { WorkDays = 10 + offset, PriceDifference = 125.25m * (offset + 1) };
                var calculation = ProgressPaymentCalculator.Calculate(input);
                var period = new DateOnly(2026, 9, 1).AddMonths(offset);
                period = new DateOnly(period.Year, period.Month, DateTime.DaysInMonth(period.Year, period.Month));
                var current = new ProgressPaymentHistoryData(Guid.NewGuid(), period.ToDateTime(TimeOnly.MinValue), input, calculation,
                    offset + 1, period, "1", "QA Route", "QA Work", "QA Contractor", "QA Tender", "QA Contract", ContractorTaxOrIdentityNumber: "12345678901");
                history.Add(current);
                var cells = program == TransportationProgramType.SpecialEducation
                    ? new Dictionary<string, decimal> { ["K4"] = calculation.TransportNetAmount, ["L4"] = calculation.TransportVat,
                        ["M4"] = calculation.TransportWithholding, ["T4"] = calculation.GuideNetAmount, ["U4"] = calculation.GuideVat,
                        ["V4"] = calculation.GuideWithholding, ["X4"] = calculation.GuideGrossAmount, ["AC4"] = calculation.AdditionalGuarantee,
                        ["AD4"] = calculation.StampTax, ["AE4"] = calculation.TotalDeductions, ["AF4"] = calculation.GrossAmount, ["AG4"] = calculation.NetPayable }
                    : new Dictionary<string, decimal> { ["K4"] = calculation.TransportNetAmount, ["L4"] = calculation.TransportVat,
                        ["M4"] = calculation.TransportWithholding, ["T4"] = calculation.AdditionalGuarantee, ["U4"] = calculation.StampTax,
                        ["V4"] = calculation.TotalDeductions, ["W4"] = calculation.NetPayable };
                void Equal(string cell, decimal actual, decimal expected) => Assert.True(Math.Abs(actual - expected) < 0.000001m,
                    $"{program}/{period}/{cell}: Excel={expected}; OEHS={actual}");
                foreach (var cell in cells) Equal(cell.Key, cell.Value, sample.GetProperty("Calculation").GetProperty(cell.Key).GetDecimal());
                var totals = ProgressPaymentReportService.Calculate(current, history);
                var report = new Dictionary<string, decimal> { ["D6"] = totals.CumulativeServiceAmount, ["D7"] = totals.CurrentPriceDifference,
                    ["D8"] = totals.CumulativeTotal, ["D10"] = totals.PreviousTotal, ["D11"] = totals.CurrentTotal,
                    ["D12"] = calculation.Vat, ["D13"] = totals.CurrentTotal + calculation.Vat,
                    ["D15"] = ProgressPaymentReportService.CalculateReportStampTax(calculation, program),
                    ["D26"] = ProgressPaymentReportService.CalculateReportTotalDeductions(calculation, program),
                    ["D28"] = ProgressPaymentReportService.CalculateReportNetPayable(current, totals) };
                foreach (var field in report)
                {
                    var expected = sample.GetProperty("Report").GetProperty(field.Key);
                    if (expected.ValueKind == JsonValueKind.Number) Equal("Report/" + field.Key, field.Value, expected.GetDecimal());
                    else
                    {
                        // Both original XLSM home-sheet lookup lists stop at June.
                        // Their July D10 is blank. OEHS intentionally retains the
                        // fiscal-year history instead of discarding prior work.
                        Assert.Equal(7, period.Month);
                        Assert.Equal("D10", field.Key);
                        Assert.Equal("", expected.GetString());
                        var independentPriorTotal = history.Where(p => p.Id != current.Id).Sum(p =>
                            p.Input.WorkDays * p.Input.DailyPrice - p.Input.DiscountAmount + p.Input.PriceDifference
                            + (program == TransportationProgramType.SpecialEducation
                                ? p.Input.GuideWorkDays * p.Input.GuideDailyPrice - p.Input.GuideDiscountAmount + p.Input.GuidePriceDifference : 0m));
                        Equal("July prior history (source lookup unavailable)", totals.PreviousTotal, independentPriorTotal);
                    }
                }
                Equal("E21", ProgressPaymentIcmalService.FindPreviousDeduction(history, current), sample.GetProperty("E21").GetDecimal());
            }
        }
    }
}
