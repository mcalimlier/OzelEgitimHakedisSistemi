using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    [Fact]
    public void FinalExcelFields_ShouldMatchAllReportAndCorrectedIcmalAmountsInEightPdfs()
    {
        using var source = JsonDocument.Parse(File.ReadAllText(LoadProjectFile("tests", "OzelEgitimHakedis.Wpf.Tests", "Fixtures", "excel-final-document-fields-2026-10-25.json")));
        RunOnSta(() =>
        {
            foreach (var sample in source.RootElement.EnumerateArray())
            {
                var program = Enum.Parse<TransportationProgramType>(sample.GetProperty("Program").GetString()!);
                var input = sample.GetProperty("Input").Deserialize<ProgressPaymentInput>()!;
                var calculation = ProgressPaymentCalculator.Calculate(input);
                var payments = new List<ProgressPaymentHistoryData>();
                var period = new DateOnly(2026, 11, 30);
                foreach (var month in sample.GetProperty("Months").EnumerateArray())
                {
                    var offset = month.GetProperty("Offset").GetInt32();
                    var date = new DateOnly(2026, 9, 1).AddMonths(offset);
                    date = new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
                    var prior = new ProgressPaymentInput(1m,
                        month.GetProperty("Amount").GetDecimal() - month.GetProperty("Difference").GetDecimal() - month.GetProperty("GuideDifference").GetDecimal(),
                        0m, month.GetProperty("Difference").GetDecimal(), 100000m, true, 0m, 0m, 0m,
                        GuidePriceDifference: month.GetProperty("GuideDifference").GetDecimal(), ProgramType: program);
                    var selected = date == period ? input : prior;
                    var result = date == period ? calculation : ProgressPaymentCalculator.Calculate(prior) with
                    { TotalDeductions = month.GetProperty("Deduction").GetDecimal() };
                    payments.Add(new ProgressPaymentHistoryData(Guid.NewGuid(), date.ToDateTime(TimeOnly.MinValue), selected, result,
                        offset + 1, date, "1", "Kaynak Test Güzergâhı", "", "Örnek Yüklenici", "QA-IKN", "QA-CONTRACT",
                        ContractorTaxOrIdentityNumber: "QA-IDENTITY"));
                }
                var current = payments.Single(p => p.PeriodDate == period);
                var totals = ProgressPaymentReportService.Calculate(current, payments);
                var reportDeduction = ProgressPaymentReportService.CalculateReportTotalDeductions(calculation, program);
                var net = ProgressPaymentReportService.CalculateReportNetPayable(current, totals);
                var expected = sample.GetProperty("Expected");
                var report = expected.GetProperty("Hakediş Raporu");
                var actual = new Dictionary<string, decimal>
                {
                    ["D6"] = totals.CumulativeServiceAmount, ["D7"] = totals.CurrentPriceDifference,
                    ["D8"] = totals.CumulativeTotal, ["D10"] = totals.PreviousTotal, ["D11"] = totals.CurrentTotal,
                    ["D12"] = calculation.Vat, ["D13"] = totals.CurrentTotal + calculation.Vat,
                    ["D14"] = 0, ["D15"] = ProgressPaymentReportService.CalculateReportStampTax(calculation, program),
                    ["D16"] = calculation.Withholding, ["D17"] = calculation.Deductions.SocialSecurityDebt,
                    ["D18"] = 0, ["D19"] = 0, ["D20"] = 0, ["D21"] = calculation.AdditionalGuarantee,
                    ["D22"] = calculation.Deductions.TaxDebt, ["D23"] = 0,
                    ["D24"] = calculation.Deductions.InspectionPenalty, ["D25"] = calculation.Deductions.MinimumWageSupport,
                    ["D26"] = reportDeduction, ["D28"] = net
                };
                foreach (var field in report.EnumerateObject())
                    Assert.True(Math.Abs(actual[field.Name] - field.Value.GetDecimal()) < 0.000001m,
                        $"{program}/Report/{field.Name}: Excel={field.Value}, OEHS={actual[field.Name]}");
                var previousDeduction = ProgressPaymentIcmalService.FindPreviousDeduction(payments, current);
                Assert.True(Math.Abs(previousDeduction - expected.GetProperty("Hakediş İcmali").GetProperty("E21").GetDecimal()) < 0.000001m);
                var contract = sample.GetProperty("Contract");
                DateOnly Date(string key) => DateOnly.Parse(contract.GetProperty(key).GetString()!, CultureInfo.InvariantCulture);
                var context = new OehsOfficialDocumentContext("", "", "1", "Örnek Yüklenici", "", contract.GetProperty("Amount").GetDecimal(),
                    Date("TenderDate"), Date("ContractDate"), Date("Start"), Date("End"), contract.GetProperty("WorkDays").GetInt32(),
                    period, 3, calculation, payments, totals, net, previousDeduction,
                    DateOnly.Parse(sample.GetProperty("GeneratedDate").GetString()!, CultureInfo.InvariantCulture), ProgramType: program, RouteName: "Kaynak Test Güzergâhı");
                var outputRoot = Environment.GetEnvironmentVariable("OEHS_FINAL_PDF_DIR");
                var directory = outputRoot is null ? Directory.CreateTempSubdirectory("oehs-final-excel-").FullName : outputRoot;
                Directory.CreateDirectory(directory);
                try
                {
                    var outputs = OehsOfficialDocumentFactory.BuildSeparate(context);
                    Assert.Equal(4, outputs.Count);
                    foreach (var output in outputs)
                    {
                        var path = Path.Combine(directory, program + "_OEHS_" + output.FileSuffix + ".pdf");
                        PdfVectorWriter.Save(output.Document, path);
                        using var pdf = UglyToad.PdfPig.PdfDocument.Open(path);
                        Assert.Single(pdf.GetPages());
                        var text = string.Concat(pdf.GetPage(1).Letters.Select(l => l.Value));
                        string Compact(string value) => string.Concat(value.Where(c => !char.IsWhiteSpace(c)));
                        Assert.Contains(Compact("Kaynak Test Güzergâhı"), Compact(text));
                        Assert.Contains(Compact(context.PreparerName), Compact(text));
                        Assert.Contains(Compact(context.ControllerName), Compact(text));
                        Assert.Contains(Compact(context.ApproverName), Compact(text));
                        Assert.Contains(context.GeneratedDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), text);
                        if (output.Kind is OehsOfficialDocumentKind.KabulTutanagi or OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu)
                        {
                            Assert.Contains(Compact(context.ContractorName), Compact(text));
                            Assert.Contains(context.ContractDate!.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), text);
                            Assert.Contains(context.ContractEndDate!.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), text);
                            Assert.Contains(context.ContractAmount.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")), text);
                        }
                        if (output.Kind == OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu)
                            Assert.Contains(context.ContractStartDate!.Value.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture), text);
                        var financialSheet = output.Kind == OehsOfficialDocumentKind.HakedisRaporu ? "Hakediş Raporu"
                            : output.Kind == OehsOfficialDocumentKind.HakedisIcmali ? "Hakediş İcmali" : null;
                        if (financialSheet is not null)
                            foreach (var field in expected.GetProperty(financialSheet).EnumerateObject())
                            {
                                var digits = output.Kind == OehsOfficialDocumentKind.HakedisRaporu && program == TransportationProgramType.SpecialEducation && field.Name == "D10" ? 3 : 2;
                                var value = field.Value.GetDecimal();
                                if (financialSheet == "Hakediş İcmali" && program == TransportationProgramType.BasicOrSecondary)
                                {
                                    // Preserve source fixture verbatim, but explicitly remove its erroneous
                                    // monthly deductions from the icmal price difference and dependent totals.
                                    var months = sample.GetProperty("Months").EnumerateArray().ToArray();
                                    var sourceDeductions = field.Name switch
                                    {
                                        "D6" or "E6" => months[0].GetProperty("Deduction").GetDecimal(),
                                        "D7" or "E7" => months[1].GetProperty("Deduction").GetDecimal(),
                                        "D8" or "E8" => months[2].GetProperty("Deduction").GetDecimal(),
                                        "D17" or "E17" or "E25" => months.Sum(m => m.GetProperty("Deduction").GetDecimal()),
                                        _ => 0m
                                    };
                                    value -= sourceDeductions;
                                }
                                var money = Math.Round(value, digits, MidpointRounding.AwayFromZero).ToString("N" + digits, CultureInfo.GetCultureInfo("tr-TR"));
                                Assert.True(Compact(text).Contains(Compact(money), StringComparison.Ordinal), $"{program}/{output.Kind}/{field.Name}: missing {money}");
                            }
                    }
                }
                finally { if (outputRoot is null) Directory.Delete(directory, true); }
            }
        });
    }
}
