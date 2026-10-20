using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Historical workbook formula vectors captured from the 2026-10-17 XLSM copies.
/// They supplement (and do not replace) the newer source vectors. The workbook
/// formulas were recalculated with LibreOffice; Excel desktop/VBA was not run.
/// </summary>
public sealed class HistoricalWorkbookFormulaParityTests
{
    [Fact]
    public void HistoricalBasicWorkbook_Rows4Through8_ShouldMatchRecalculatedFormulaCells()
    {
        var examples = new[]
        {
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 9.82693033060048m,
                    DailyPrice: 19539.1608m,
                    DiscountAmount: 0m,
                    PriceDifference: 7365.349999999999m,
                    ContractAmount: 100000m,
                    IsVatExempt: false,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    ProgramType: TransportationProgramType.BasicOrSecondary),
                K = 199375.32189999995m,
                L = 19937.532189999994m,
                M = 9968.766094999997m,
                N = 209344.08799499995m,
                O = 219312.85408999995m,
                T = 441.921m,
                U = 1890.0780516119996m,
                V = 12300.765146611997m,
                W = 207012.08894338796m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 13m,
                    DailyPrice: 6545m,
                    DiscountAmount: 0m,
                    PriceDifference: 3659.2m,
                    ContractAmount: 100000m,
                    IsVatExempt: true,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    ProgramType: TransportationProgramType.BasicOrSecondary),
                K = 88744.2m, L = 0m, M = 0m, N = 88744.2m, O = 88744.2m,
                T = 219.552m, U = 841.295016m, V = 1060.847016m, W = 87683.352984m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 13m,
                    DailyPrice: 6135m,
                    DiscountAmount: 0m,
                    PriceDifference: 3429.97m,
                    ContractAmount: 100000m,
                    IsVatExempt: true,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    ProgramType: TransportationProgramType.BasicOrSecondary),
                K = 83184.97m, L = 0m, M = 0m, N = 83184.97m, O = 83184.97m,
                T = 205.7982m, U = 788.5935156m, V = 994.3917156m, W = 82190.5782844m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 6.83275008785484m,
                    DailyPrice: 4857.1601m,
                    DiscountAmount: 0m,
                    PriceDifference: 3649.97m,
                    ContractAmount: 100000m,
                    IsVatExempt: true,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    ProgramType: TransportationProgramType.BasicOrSecondary),
                K = 36837.731100000026m, L = 0m, M = 0m, N = 36837.731100000026m, O = 36837.731100000026m,
                T = 218.9982m, U = 349.22169082800025m, V = 568.2198908280002m, W = 36269.51120917203m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 13m,
                    DailyPrice: 2810m,
                    DiscountAmount: 0m,
                    PriceDifference: 1571.03m,
                    ContractAmount: 100000m,
                    IsVatExempt: false,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    ProgramType: TransportationProgramType.BasicOrSecondary),
                K = 38101.03m, L = 3810.103m, M = 1905.0515m, N = 40006.0815m, O = 41911.133m,
                T = 94.2618m, U = 361.1977644m, V = 2360.5110644m, W = 39550.6219356m
            }
        };

        foreach (var example in examples)
        {
            var result = ProgressPaymentCalculator.Calculate(example.Input);

            AssertNear(example.K, result.TransportNetAmount);
            AssertNear(example.L, result.TransportVat);
            AssertNear(example.M, result.Withholding);
            AssertNear(example.N, result.InvoiceAmountAfterWithholding);
            AssertNear(example.O, result.GrossAmount);
            AssertNear(example.T, result.AdditionalGuarantee);
            AssertNear(example.U, result.StampTax);
            AssertNear(example.V, result.TotalDeductions);
            AssertNear(example.W, result.NetPayable);
        }
    }

    [Fact]
    public void HistoricalSpecialEducationWorkbook_Rows4Through6_ShouldMatchRecalculatedFormulaCells()
    {
        var examples = new[]
        {
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 22m,
                    DailyPrice: 2320m,
                    DiscountAmount: 0m,
                    PriceDifference: 1403.573m,
                    ContractAmount: 100000m,
                    IsVatExempt: true,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    GuideWorkDays: 30m,
                    GuideDailyPrice: 1362.49m,
                    GuideDiscountAmount: 0m,
                    GuidePriceDifference: 0m,
                    ProgramType: TransportationProgramType.SpecialEducation),
                K = 52443.573m, L = 0m, M = 0m, N = 52443.573m, O = 52443.573m,
                T = 40874.7m, U = 0m, V = 0m, W = 40874.7m, X = 40874.7m,
                AC = 84.21438m, AD = 884.65722804m, AE = 968.87160804m,
                AF = 93318.273m, AG = 92349.40139196m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 22m,
                    DailyPrice: 1894m,
                    DiscountAmount: 0m,
                    PriceDifference: 1145.848m,
                    ContractAmount: 100000m,
                    IsVatExempt: true,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    GuideWorkDays: 30m,
                    GuideDailyPrice: 1362.49m,
                    GuideDiscountAmount: 0m,
                    GuidePriceDifference: 0m,
                    ProgramType: TransportationProgramType.SpecialEducation),
                K = 42813.848m, L = 0m, M = 0m, N = 42813.848m, O = 42813.848m,
                T = 40874.7m, U = 0m, V = 0m, W = 40874.7m, X = 40874.7m,
                AC = 68.75088m, AD = 793.36743504m, AE = 862.11831504m,
                AF = 83688.548m, AG = 82826.42968496m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 22m,
                    DailyPrice: 2184m,
                    DiscountAmount: 0m,
                    PriceDifference: 1321.2946m,
                    ContractAmount: 100000m,
                    IsVatExempt: true,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    GuideWorkDays: 30m,
                    GuideDailyPrice: 1362.49m,
                    GuideDiscountAmount: 0m,
                    GuidePriceDifference: 0m,
                    ProgramType: TransportationProgramType.SpecialEducation),
                K = 49369.2946m, L = 0m, M = 0m, N = 49369.2946m, O = 49369.2946m,
                T = 40874.7m, U = 0m, V = 0m, W = 40874.7m, X = 40874.7m,
                AC = 79.277676m, AD = 855.513068808m, AE = 934.790744808m,
                AF = 90243.9946m, AG = 89309.203855192m
            }
        };

        foreach (var example in examples)
        {
            var result = ProgressPaymentCalculator.Calculate(example.Input);

            AssertNear(example.K, result.TransportNetAmount); 
            AssertNear(example.L, result.TransportVat);
            AssertNear(example.M, result.TransportWithholding);
            AssertNear(example.N, result.TransportNetAmount + result.TransportWithholding);
            AssertNear(example.O, result.TransportNetAmount + result.TransportVat);
            AssertNear(example.T, result.GuideNetAmount);
            AssertNear(example.U, result.GuideVat);
            AssertNear(example.V, result.GuideWithholding);
            AssertNear(example.W, result.GuideNetAmount + result.GuideWithholding);
            AssertNear(example.X, result.GuideNetAmount + result.GuideVat);
            AssertNear(example.AC, result.AdditionalGuarantee);
            AssertNear(example.AD, result.StampTax);
            AssertNear(example.AE, result.TotalDeductions);
            AssertNear(example.AF, result.GrossAmount);
            AssertNear(example.AG, result.NetPayable);
        }
    }

    private static void AssertNear(decimal expected, decimal actual) =>
        Assert.InRange(Math.Abs(expected - actual), 0m, 0.000001m);
}
