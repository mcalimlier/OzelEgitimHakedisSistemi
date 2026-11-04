using OzelEgitimHakedis.Core.Models;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class OfficialTemplateProfileTests
{
    [Theory]
    [InlineData(TransportationProgramType.BasicOrSecondary, "Hesaplama Bilgileri", "Hizmet İşleri Hakediş Raporu", null)]
    [InlineData(TransportationProgramType.SpecialEducation, "Hesaplama", "Hizmetişleri Hakediş Raporu", "Rehber Personel Fiyat Farkı KDV Hariç")]
    public void ProgramProfile_ShouldUseExactOfficialWorkbookTerminology(
        TransportationProgramType program, string calculationSheet, string serviceReport, string? guideDifference)
    {
        var profile = OfficialTemplateProfile.For(program);
        Assert.Equal(calculationSheet, profile.CalculationSheetName);
        Assert.Equal(serviceReport, profile.ServiceReportSheetName);
        Assert.Equal("Hattın Adı", profile.RouteNameLabel);
        Assert.Equal("Taşıma Fiyat Farkı KDV Hariç", profile.TransportPriceDifferenceLabel);
        Assert.Equal(guideDifference, profile.GuidePriceDifferenceLabel);
        Assert.Equal("Ele Geçen", profile.NetPayableLabel);
    }
}
