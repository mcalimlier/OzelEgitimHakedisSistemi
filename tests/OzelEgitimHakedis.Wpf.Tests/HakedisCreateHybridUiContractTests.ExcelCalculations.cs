using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    private static readonly bool[] ExportNetVisibilityCases = {false,true};
    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation,false)]
    [InlineData(TransportationProgramType.SpecialEducation,true)]
    [InlineData(TransportationProgramType.BasicOrSecondary,false)]
    [InlineData(TransportationProgramType.BasicOrSecondary,true)]
    public void HybridGridExcel_ShouldAppendLiveCalculatedAmountsForEveryRoute(TransportationProgramType program,bool exempt)
    {
        RunOnSta(() =>
        {
            var temp=Directory.CreateTempSubdirectory("oehs-calculated-export-");
            try
            {
                var window=CreateHybridUpdateHarness(Path.Combine(temp.FullName,"data"));
                var rows=Field<ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window,"_multiRoutePaymentRows");
                var row=Assert.Single(rows);
                row.Configure(program,500000m,exempt,ProgressPaymentCalculationConstants.Defaults with
                {TransportVatRate=.13m,GuideVatRate=.22m,TransportWithholdingShare=.7m,GuideWithholdingShare=.8m,StampTaxRate=.012m,AdditionalGuaranteeRate=.09m});
                row.WorkDays=10;row.DailyPrice=1000;row.PriceDifference=500;row.DiscountAmount=100;
                row.GuideWorkDays=6;row.GuideDailyPrice=100;row.GuidePriceDifference=50;row.GuideDiscountAmount=10;
                row.TaxDebt=10;row.SocialSecurityDebt=20;row.MinimumWageSupport=30;row.InspectionPenalty=40;
                var second=new MainWindow.MultiRoutePaymentDraftRow{RouteName="İkinci hat",WorkDays=5,DailyPrice=500};
                second.Configure(program,500000m,exempt,ProgressPaymentCalculationConstants.Defaults);
                rows.Add(second);
                var grid=Field<DataGrid>(window,"MultiRoutePaymentGrid");grid.AutoGenerateColumns=false;grid.ItemsSource=rows;
                grid.Columns.Add(new DataGridTextColumn{Header="Hattın Adı",Binding=new Binding(nameof(row.RouteName))});
                foreach(var showNet in ExportNetVisibilityCases)
                {
                    if(showNet){row.DailyPrice=1000.1234m;row.PriceDifference=500.12345m;}
                    if(showNet)grid.Columns.Add(new DataGridTextColumn{Header="Net Ödenecek (TL)",Binding=new Binding(nameof(row.NetPayableText))});
                    var path=Path.Combine(temp.FullName,$"result-{showNet}.xlsx");
                    Assert.True((bool)Invoke(window,"SaveHybridTableExcel",path)!);
                    using var zip=ZipFile.OpenRead(path);using var input=zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
                    var sheet=XDocument.Load(input);XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
                    var sheetRows=sheet.Descendants(ns+"row").ToArray();
                    var headers=sheetRows[0].Descendants(ns+"t").Select(t=>t.Value).ToArray();
                    using var styleInput=zip.GetEntry("xl/styles.xml")!.Open();var styles=XDocument.Load(styleInput);
                    var formats=styles.Descendants(ns+"cellXfs").Single().Elements(ns+"xf").ToArray();
                    Assert.Equal("4",(string?)formats[6].Attribute("numFmtId"));
                    Assert.Equal(3,sheetRows.Length);Assert.Equal("Hattın Adı",headers[0]);
                    Assert.Single(headers.Where(h=>h=="Net Ödenecek (TL)"));
                    for(var i=0;i<rows.Count;i++)
                    {
                        var result=Assert.IsType<ProgressPaymentCalculation>(rows[i].Preview);
                        var cells=sheetRows[i+1].Elements(ns+"c").ToArray();
                        decimal Amount(string header)
                        {
                            var index=Array.IndexOf(headers,header);Assert.True(index>=0,"Eksik hesap sütunu: "+header);
                            Assert.Null(cells[index].Attribute("t"));
                            Assert.Equal("6",(string?)cells[index].Attribute("s"));
                            return decimal.Parse(cells[index].Element(ns+"v")!.Value,CultureInfo.InvariantCulture);
                        }
                        Assert.Equal(result.ServiceAmount,Amount("Taşıma Hizmet Tutarı (TL)"));
                        Assert.Equal(result.PriceDifferenceExcludingVat,Amount("Taşıma Fiyat Farkı (KDV Hariç) (TL)"));
                        Assert.Equal(result.GuideServiceAmount,Amount("Rehber Hizmet Tutarı (TL)"));
                        Assert.Equal(result.GuidePriceDifferenceExcludingVat,Amount("Rehber Fiyat Farkı (KDV Hariç) (TL)"));
                        Assert.Equal(result.TransportNetAmount,Amount("Taşıma Hakediş Tutarı (TL)"));
                        Assert.Equal(result.GuideNetAmount,Amount("Rehber Hakediş Tutarı (TL)"));
                        Assert.Equal(result.TotalAmount,Amount("Toplam Hakediş (KDV Hariç) (TL)"));
                        Assert.Equal(result.Vat,Amount("KDV (TL)"));Assert.Equal(result.Withholding,Amount("KDV Tevkifatı (TL)"));
                        Assert.Equal(result.InvoiceAmountAfterWithholding,Amount("Fatura Tutarı (Tevkifat Sonrası) (TL)"));
                        Assert.Equal(result.StampTax,Amount("Damga Vergisi (TL)"));
                        Assert.Equal(result.AdditionalGuarantee,Amount("Ek Kesin Teminat (TL)"));
                        Assert.Equal(result.Deductions.TaxDebt,Amount("Kesinti: Vergi Borcu (TL)"));
                        Assert.Equal(result.Deductions.SocialSecurityDebt,Amount("Kesinti: SGK Borcu (TL)"));
                        Assert.Equal(result.Deductions.MinimumWageSupport,Amount("Kesinti: Asgari Ücret Desteği (TL)"));
                        Assert.Equal(result.Deductions.InspectionPenalty,Amount("Kesinti: Denetim Cezası (TL)"));
                        Assert.Equal(result.GrossAmount,Amount("Brüt Tutar (KDV Dahil) (TL)"));
                        Assert.Equal(result.TotalDeductions,Amount("Toplam Kesinti (TL)"));
                        Assert.Equal(result.NetPayable,Amount("Net Ödenecek (TL)"));
                        Assert.Equal(Amount("Brüt Tutar (KDV Dahil) (TL)")-Amount("Toplam Kesinti (TL)"),Amount("Net Ödenecek (TL)"));
                    }
                    if(program==TransportationProgramType.SpecialEducation && !exempt && !showNet)
                    {Assert.Equal(132.48m,row.Preview!.StampTax);Assert.Equal(49.5m,row.Preview.AdditionalGuarantee);Assert.Equal(1492.8m,row.Preview.Vat);Assert.Equal(11191.78m,row.Preview.NetPayable);}
                    Assert.True(row.HasChanges);Assert.Empty(new OzelEgitimHakedis.Core.Services.OehsProgramScopedApplicationDataStore(Path.Combine(temp.FullName,"data")).Load().Routes);
                }
            }
            finally{temp.Delete(true);}
        });
    }

    [Fact]
    public void HybridGridExcel_ShouldKeepExistingFileWhenCalculatedRowIsInvalid()
    {
        RunOnSta(() =>
        {
            var temp=Directory.CreateTempSubdirectory("oehs-invalid-export-");
            try
            {
                var window=CreateHybridUpdateHarness(Path.Combine(temp.FullName,"data"));
                Field<ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window,"_multiRoutePaymentRows").Single().WorkDays=-1;
                var path=Path.Combine(temp.FullName,"existing.xlsx");File.WriteAllText(path,"existing workbook");
                Assert.False((bool)Invoke(window,"SaveHybridTableExcel",path)!);
                Assert.Equal("existing workbook",File.ReadAllText(path));
            }
            finally{temp.Delete(true);}
        });
    }
}
