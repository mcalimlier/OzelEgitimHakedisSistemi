using System.IO;
using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    [Fact]
    public void SuggestedRates_ShouldUseLatestSavedRevisionWithinTheMonth()
    {
        RunOnSta(() =>
        {
            var window=CreateHybridUpdateHarness("unused-period-prices");
            var row=Assert.Single(Field<System.Collections.ObjectModel.ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window,"_multiRoutePaymentRows"));
            ProgressPaymentInput Rate(decimal price)=>PaymentRatePeriodService.Apply(row.CreateInput(),new[]{new PaymentRatePeriod(18,price)},null);
            var first=Rate(1200);var pending=Rate(1600);var revised=Rate(1800);
            ProgressPaymentHistoryData Payment(ProgressPaymentInput input,int day)=>new(Guid.NewGuid(),new DateTime(2026,10,day,12,0,0,DateTimeKind.Utc),input,
                ProgressPaymentCalculator.Calculate(input),1,new DateOnly(2026,10,31),"01","Merkez","Merkez","Firma","IHALE-01","SOZ-01",ContractorTaxOrIdentityNumber:"12345678901");
            Collection(window,"_progressHistory").Add(Payment(first,2));
            SetField(window,"_savedHybridPaymentDrafts",new List<HybridPaymentDraftData>{new(row.RouteKey,new DateOnly(2026,10,1),pending,SavedAt:new DateTime(2026,10,15,12,0,0,DateTimeKind.Utc))});
            var months=Field<ComboBox>(window,"ProgressPeriodMonthComboBox");
            var november=new ProgressPaymentMonthOption(new DateOnly(2026,11,1),new DateOnly(2026,11,30));months.Items.Add(november);months.SelectedItem=november;
            Invoke(window,"SuggestPeriodDailyPrices",row,Collection(window,"_routes")[0]!);
            Assert.Equal(1600m,row.DailyPrice);
            Collection(window,"_progressHistory").Add(Payment(revised,20));
            Invoke(window,"SuggestPeriodDailyPrices",row,Collection(window,"_routes")[0]!);
            Assert.Equal(1800m,row.DailyPrice);
            Assert.Equal(0m,row.PriceDifference);Assert.Null(row.TransportPeriods);
            SetField(window,"_savedHybridPaymentDrafts",new List<HybridPaymentDraftData>{new(row.RouteKey,new DateOnly(2026,10,1),row.CreateInput() with {DailyPrice=1000,HasPeriodChanges=false})});
            months.SelectedItem=months.Items.OfType<ProgressPaymentMonthOption>().First(p=>p.PeriodDate.Month==10);
            Invoke(window,"RestoreSavedHybridInput",row);
            Assert.Equal(1800m,row.DailyPrice);
            Assert.True(row.HasPeriodChanges);
        });
    }

    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation)]
    [InlineData(TransportationProgramType.BasicOrSecondary)]
    public void PeriodDialog_ShouldKeepOriginalUntilApplyAndSupportIndependentPriceDifferences(TransportationProgramType program)
    {
        RunOnSta(() =>
        {
            var input = new ProgressPaymentInput(18,1000,100,0,100000,false,50,0,0,
                GuideWorkDays:18,GuideDailyPrice:100,ProgramType:program);
            var dialog = new PaymentPeriodChangeWindow(input,
                new ProgressPaymentMonthOption(new DateOnly(2026,10,1),new DateOnly(2026,10,31)), "Hat 01");
            dialog.TransportRows[0].DaysText="10"; dialog.TransportRows[0].PriceDifferenceText="100";
            dialog.TransportRows.Add(new PaymentPeriodEntry(8,1200,200));
            Assert.Null(dialog.ResultInput);
            Assert.Null(input.TransportPeriods);
            if(program==TransportationProgramType.SpecialEducation)
            {
                dialog.GuideRows[0].DaysText="6"; dialog.GuideRows[0].PriceDifferenceText="30";
                dialog.GuideRows.Add(new PaymentPeriodEntry(12,150,50));
            }
            Assert.True(dialog.TryApplyChanges());
            var result=Assert.IsType<ProgressPaymentInput>(dialog.ResultInput);
            Assert.Equal(300m,result.PriceDifference);
            Assert.Equal(program==TransportationProgramType.SpecialEducation?80m:0m,result.GuidePriceDifference);
            var row=new MainWindow.MultiRoutePaymentDraftRow();
            row.Configure(program,100000,false,ProgressPaymentCalculationConstants.Defaults);
            row.LoadInput(input,true);row.AcceptChanges();row.ApplyPeriodInput(result);
            Assert.True(row.HasChanges);
            Assert.Equal(19600m,row.Preview!.ServiceAmount);
            Assert.Equal(program==TransportationProgramType.SpecialEducation?2400m:0m,row.Preview.GuideServiceAmount);
            row.ReconcileSource("Hat",1000,100);row.ReconcileSource("Hat",9999,9999);
            Assert.Equal(1200m,row.DailyPrice);
            Assert.True(row.ControlsPeriodField(nameof(row.PriceDifference)));
            Assert.False(row.ControlsPeriodField(nameof(row.TaxDebt)));
            var content=Assert.IsAssignableFrom<FrameworkElement>(dialog.Content);
            ArrangeLayout(content,880,650);
            SavePaymentDetailsPreview(content,$"period-dialog-{program}.png");
            dialog.TransportRows[0].DaysText="yanlış";
            Assert.False(dialog.TryApplyChanges());
            dialog.Close();
        });
    }

    [Fact]
    public void PeriodChanges_ShouldSaveOneClaimAndCarryOnlyFinalPricesToNextMonth()
    {
        RunOnSta(() =>
        {
            var directory=Directory.CreateTempSubdirectory("oehs-period-save-");
            try
            {
                var root=Path.Combine(directory.FullName,"data");
                var window=CreateHybridUpdateHarness(root);
                var rows=Field<System.Collections.ObjectModel.ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window,"_multiRoutePaymentRows");
                var row=Assert.Single(rows);
                row.ApplyPeriodInput(PaymentRatePeriodService.Apply(row.CreateInput(),
                    new[]{new PaymentRatePeriod(10,1000,100),new PaymentRatePeriod(8,1200,200)},
                    new[]{new PaymentRatePeriod(6,100,30),new PaymentRatePeriod(12,150,50)}));
                var grid=Field<DataGrid>(window,"MultiRoutePaymentGrid");
                grid.Columns.Add(new DataGridTextColumn{Header="Hat",Binding=new System.Windows.Data.Binding(nameof(row.RouteDisplayText))});
                grid.Columns.Add(new DataGridTemplateColumn{Header="İşlem"});
                var excel=Path.Combine(directory.FullName,"sections.xlsx");
                Assert.True((bool)Invoke(window,"SaveHybridTableExcel",excel)!);
                using(var zip=System.IO.Compression.ZipFile.OpenRead(excel))
                using(var xml=zip.GetEntry("xl/worksheets/sheet1.xml")!.Open())
                {
                    var content=System.Xml.Linq.XDocument.Load(xml).Root!.Value;
                    Assert.Contains("Dönem içi hesap ayrıntısı",content,StringComparison.Ordinal);
                    Assert.Contains("10 × 1.000",content,StringComparison.Ordinal);
                }
                Invoke(window,"CalculateProgressButton_Click",window,new RoutedEventArgs());
                var payment=Assert.Single(new OehsProgramScopedApplicationDataStore(root).Load().ProgressPayments!);
                Assert.Equal(2,payment.Input.TransportPeriods!.Count);
                Assert.Equal(2,payment.Input.GuidePeriods!.Count);
                Assert.Equal(19600m,payment.Calculation.ServiceAmount);
                var context=Assert.IsType<OehsOfficialDocumentContext>(Invoke(window,"BuildOfficialDocumentContextForPayment",payment));
                var pdfPath=Path.Combine(directory.FullName,"period-change.pdf");
                PdfVectorWriter.Save(OehsOfficialDocumentFactory.BuildCombined(context),pdfPath);
                using(var pdf=UglyToad.PdfPig.PdfDocument.Open(pdfPath))
                { Assert.Equal(4,pdf.NumberOfPages);Assert.Contains("22.000,00",pdf.GetPage(1).Text,StringComparison.Ordinal); }
                var month=Field<ComboBox>(window,"ProgressPeriodMonthComboBox");
                var november=new ProgressPaymentMonthOption(new DateOnly(2026,11,1),new DateOnly(2026,11,30));
                month.Items.Add(november);
                month.SelectedItem=november;
                Invoke(window,"ResetMonthlyInputsForNewMonth");
                var selector=Field<ListBox>(window,"ProgressRouteComboBox");
                selector.ItemsSource=null;selector.Items.Clear();
                var route=Collection(window,"_routes")[0]!;
                selector.Items.Add(route);selector.SelectedItems.Add(route);
                Invoke(window,"UpdateHybridRouteDraftRows");
                var next=Assert.Single(Field<System.Collections.ObjectModel.ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window,"_multiRoutePaymentRows"));
                Assert.Equal(0m,next.WorkDays);Assert.Equal(0m,next.PriceDifference);Assert.Null(next.TransportPeriods);
                Assert.Equal(1200m,next.DailyPrice);Assert.Equal(150m,next.GuideDailyPrice);
                Assert.Equal(300m,payment.Input.PriceDifference);
            }
            finally {directory.Delete(true);}
        });
    }
}
