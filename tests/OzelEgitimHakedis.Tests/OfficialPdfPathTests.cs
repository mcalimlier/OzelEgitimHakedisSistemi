using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class OfficialPdfPathTests
{
    private static ProgressPaymentHistoryData Payment()
    {
        var input = new ProgressPaymentInput(1, 100, 0, 0, 100000, true, 0, 0, 0);
        return new(Guid.NewGuid(), DateTime.UtcNow, input, ProgressPaymentCalculator.Calculate(input),
            1, new DateOnly(2026, 10, 31), "01", new string('X', 300),
            ContractorName: "Sınırlı Sorumlu Yeşil Sivaslı Minibüsçüler Karayolu ile Yolcu Taşıyıcılar Kooperatifi",
            TenderNumber: "2026/1562223", ContractNumber: "SOZ-01", ContractorTaxOrIdentityNumber: "12345678901");
    }

    [Fact]
    public void OfficialPaths_ShouldUseTenderAndContractorWithoutLongRouteName()
    {
        var payment = Payment();
        var root = @"C:\Users\MuratPC\Desktop\OzelEgitimHakedisSistemi";
        var path = OehsOutputPathService.BuildOfficialPdfFilePath(root, payment, "01_Hakedis_Raporu");
        Assert.StartsWith("2026-1562223-Sınırlı Sorumlu Yeşil Sivaslı", Path.GetFileName(path), StringComparison.Ordinal);
        Assert.DoesNotContain("12345678901", path, StringComparison.Ordinal);
        Assert.DoesNotContain(payment.RouteName!, path, StringComparison.Ordinal);
        Assert.Equal(path, OehsOutputPathService.BuildOfficialPdfFilePath(root, payment with { Id = Guid.NewGuid() }, "01_Hakedis_Raporu"));
    }

    [Fact]
    public void OfficialPaths_ShouldStayShortAndSeparateRouteBatchMonthProgramAndContractor()
    {
        var payment = Payment() with { ContractorName = new string('Ş', 300), BatchRouteNumbers = new string('B', 400) };
        var root = @"C:\Users\MuratPC\Desktop\OzelEgitimHakedisSistemi";
        const string suffix = "03_Hizmet_Isleri_Kabul_Tutanagi";
        var path = OehsOutputPathService.BuildOfficialPdfFilePath(root, payment, suffix);
        Assert.InRange(path.Length, 1, 240);
        foreach (var other in new[]
        {
            payment with { RouteNumber = "02" }, payment with { BatchRouteNumbers = "02+03" },
            payment with { PeriodDate = new DateOnly(2026, 11, 30) },
            payment with { Input = payment.Input with { ProgramType = TransportationProgramType.BasicOrSecondary } },
            payment with { ContractorTaxOrIdentityNumber = "98765432101" },
            payment with { ContractNumber = "SOZ-02" }, payment with { ProgressPaymentNumber = 2 }
        })
            Assert.NotEqual(path, OehsOutputPathService.BuildOfficialPdfFilePath(root, other, suffix));
        var folder = Directory.CreateTempSubdirectory("oehs-path-");
        try
        {
            var target = OehsOutputPathService.BuildOfficialPdfFilePath(folder.FullName, payment, suffix);
            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, "path probe");
            Assert.True(File.Exists(target));
        }
        finally { folder.Delete(true); }
    }
}
