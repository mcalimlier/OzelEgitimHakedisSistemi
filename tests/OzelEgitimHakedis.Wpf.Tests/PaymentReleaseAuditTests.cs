using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    private static readonly System.Text.Json.JsonSerializerOptions ExcelFixtureJsonOptions = new JsonSerializerOptions{PropertyNameCaseInsensitive=true};
    private static readonly string[] ReleaseSelectionInputNames = new[]{"ProgressTenderListBox","ProgressContractorComboBox","ProgressPeriodMonthComboBox"};
    private static readonly System.Text.Json.JsonSerializerOptions ReleaseAuditEvidenceJsonOptions = new JsonSerializerOptions{WriteIndented=true};

    [Fact]
    public void PaymentSelectionCards_ShouldAlignInputBoundsAtSupportedSizes()
    {
        RunOnSta(() =>
        {
            foreach (var size in new[] { (1024d,600d), (1366d,768d), (1920d,1080d) })
            {
                var layout=LoadLayoutFragment(NamedElement(LoadMainWindowXaml(),"ProgressPanel"));
                ArrangeLayout(layout,size.Item1-253,size.Item2-98);
                var controls=ReleaseSelectionInputNames
                    .Select(name=>(FrameworkElement)layout.FindName(name)).ToArray();
                var bounds=controls.Select(c=>c.TransformToAncestor(layout).TransformBounds(new Rect(c.RenderSize))).ToArray();
                Assert.All(bounds,b=>Assert.InRange(System.Math.Abs(b.Top-bounds[0].Top),0,1));
                Assert.All(bounds,b=>Assert.InRange(System.Math.Abs(b.Height-bounds[0].Height),0,1));
                foreach(var pair in new[]{("WorkDaysTextBox","DailyPriceTextBox"),("GuideWorkDaysTextBox","GuideDailyPriceTextBox"),
                    ("DiscountTextBox","PriceDifferenceTextBox"),("GuideDiscountTextBox","GuidePriceDifferenceTextBox"),
                    ("TaxDebtTextBox","SocialSecurityDebtTextBox"),("MinimumWageSupportTextBox","InspectionPenaltyTextBox")})
                {
                    var first=(FrameworkElement)layout.FindName(pair.Item1);var second=(FrameworkElement)layout.FindName(pair.Item2);
                    var one=first.TransformToAncestor(layout).TransformBounds(new Rect(first.RenderSize));
                    var two=second.TransformToAncestor(layout).TransformBounds(new Rect(second.RenderSize));
                    Assert.InRange(System.Math.Abs(one.Top-two.Top),0,1);
                    Assert.InRange(System.Math.Abs(one.Height-two.Height),0,1);
                }
                var output=System.Environment.GetEnvironmentVariable("OEHS_PAYMENT_QA_OUTPUT");
                if(!string.IsNullOrEmpty(output)&&size.Item1==1366)
                {
                    Directory.CreateDirectory(output);
                    var bmp=new System.Windows.Media.Imaging.RenderTargetBitmap((int)layout.ActualWidth,(int)layout.ActualHeight,96,96,PixelFormats.Pbgra32);bmp.Render(layout);
                    var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
                    using var file=File.Create(Path.Combine(output,"payment-panel-aligned.png"));encoder.Save(file);
                }
            }
        });
    }

    private sealed record ExcelAuditCase(string Program,string Scenario,int Row,ProgressPaymentInput Input,
        Dictionary<string,decimal> Expected,Dictionary<string,decimal> ExpectedRounded,string Sha256);
    private sealed record ExcelAuditTotal(string Program,string Field,decimal ExcelSum,decimal ExcelMoney);

    private static ExcelAuditCase[] ReadExcelAuditCases() => JsonSerializer.Deserialize<ExcelAuditCase[]>(
        File.ReadAllText(LoadProjectFile("tests","OzelEgitimHakedis.Wpf.Tests","Fixtures","excel-recalculated-2026-10-25.json")),
        ExcelFixtureJsonOptions)!;

    private static void AssertExcelResult(ExcelAuditCase sample,ProgressPaymentCalculation calculation)
    {
        foreach(var (field,expected) in sample.Expected)
        {
            var actual=(decimal)typeof(ProgressPaymentCalculation).GetProperty(field)!.GetValue(calculation)!;
            Assert.True(System.Math.Abs(actual-expected)<=0.000001m,
                $"{sample.Program}/{sample.Scenario}/row {sample.Row}/{field}: Excel={expected}, WPF={actual}");
            // Excel's own ROUND result is the money-display oracle. Rounding a
            // serialized binary Value2 in .NET can misclassify an exact half-cent.
            Assert.True(sample.ExpectedRounded[field]==System.Math.Round(actual,2,System.MidpointRounding.AwayFromZero),
                $"{sample.Program}/{sample.Scenario}/row {sample.Row}/{field}: Excel ROUND={sample.ExpectedRounded[field]}, WPF={System.Math.Round(actual,2,System.MidpointRounding.AwayFromZero)}");
            Assert.Equal(sample.ExpectedRounded[field].ToString("N2",CultureInfo.GetCultureInfo("tr-TR")),actual.ToString("N2",CultureInfo.GetCultureInfo("tr-TR")));
        }
    }

    [Fact]
    public void PaymentExcelCases_ShouldMatchRealRowPreviewsForEveryRecalculatedScenario()
    {
        var samples=ReadExcelAuditCases();
        Assert.Equal(24,samples.Length);
        foreach(var sample in samples)
        {
            var row=new MainWindow.MultiRoutePaymentDraftRow();
            row.Configure(sample.Input.ProgramType,sample.Input.ContractAmount,sample.Input.IsVatExempt,ProgressPaymentCalculationConstants.Defaults);
            row.LoadInput(sample.Input,true);
            Assert.Equal(sample.Input,row.CreateInput());
            AssertExcelResult(sample,Assert.IsType<ProgressPaymentCalculation>(row.Preview));
        }
    }

    [Fact]
    public void PaymentExcelSourceRows_ShouldPersistEightIndependentPaymentsThroughRealCalculateHandler()
    {
        RunOnSta(() =>
        {
            foreach(var group in ReadExcelAuditCases().Where(c=>c.Scenario=="source").GroupBy(c=>c.Input.ProgramType))
            {
                var samples=group.OrderBy(s=>s.Row).ToArray();
                var directory=Directory.CreateTempSubdirectory("oehs-release-excel-");
                try
                {
                    var window=CreateEditorHarness();var root=Path.Combine(directory.FullName,"data");
                    SetField(window,"_dataStore",new OehsProgramScopedApplicationDataStore(root));
                    SetField(window,"_archiveStore",new ProgressPaymentArchiveStore(Path.Combine(directory.FullName,"archive")));
                    SetField(window,"_calculationConstantsHistory",new List<ProgressPaymentCalculationConstantsChange>());
                    var contract=ArchiveProgressEditorContextTests.CreateArchive().Contract with {ProgramType=group.Key};
                    Collection(window,"_contracts").Add(contract);
                    var firm=System.Activator.CreateInstance(typeof(MainWindow).GetNestedType("ContractorDraft",System.Reflection.BindingFlags.NonPublic)!,
                        "Excel deneme firması","99999999990","","","","","",group.Key,false)!;
                    Collection(window,"_contractors").Add(firm);
                    var programs=Field<ComboBox>(window,"TransportProgramTypeComboBox");programs.Items.Add(new ComboBoxItem{Tag=group.Key.ToString()});programs.SelectedIndex=0;
                    var tenders=Field<ComboBox>(window,"ProgressTenderListBox");tenders.Items.Add(contract);tenders.SelectedItem=contract;
                    var routes=Field<ListBox>(window,"ProgressRouteComboBox");
                    foreach(var sample in samples)
                    {
                        var route=CreateRoute(sample.Row.ToString(CultureInfo.InvariantCulture),"Excel deneme hattı "+sample.Row,"99999999990",group.Key,
                            transportDays:sample.Input.WorkDays,dailyPrice:sample.Input.DailyPrice,guidePrice:sample.Input.GuideDailyPrice);
                        Collection(window,"_routes").Add(route);routes.Items.Add(route);routes.SelectedItems.Add(route);
                    }
                    var period=new ProgressPaymentMonthOption(new System.DateOnly(2026,11,1),new System.DateOnly(2026,11,30));
                    var periods=Field<ComboBox>(window,"ProgressPeriodMonthComboBox");periods.Items.Add(period);periods.SelectedItem=period;
                    Invoke(window,"UpdateHybridRouteDraftRows");
                    var rows=Field<ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window,"_multiRoutePaymentRows");
                    Assert.Equal(samples.Length,rows.Count);
                    for(var n=0;n<rows.Count;n++) rows[n].LoadInput(samples[n].Input,true);
                    Invoke(window,"RefreshHybridTotals");
                    var displayed=Field<TextBlock>(window,"ProgressNetSummaryTextBlock").Text;
                    Assert.Contains(samples.Sum(s=>s.Expected["NetPayable"]).ToString("N2",CultureInfo.GetCultureInfo("tr-TR")),displayed,System.StringComparison.Ordinal);
                    Invoke(window,"CalculateProgressButton_Click",window,new RoutedEventArgs());
                    var saved=new OehsProgramScopedApplicationDataStore(root).Load();
                    Assert.Equal(samples.Length,saved.ProgressPayments!.Count);
                    foreach(var payment in saved.ProgressPayments)
                    {
                        var sample=samples.Single(s=>s.Row.ToString(CultureInfo.InvariantCulture)==payment.RouteNumber);
                        Assert.Equal(sample.Input,payment.Input);
                        AssertExcelResult(sample,payment.Calculation);
                    }
                    var totals=JsonSerializer.Deserialize<ExcelAuditTotal[]>(File.ReadAllText(LoadProjectFile(
                        "tests","OzelEgitimHakedis.Wpf.Tests","Fixtures","excel-source-totals-2026-10-25.json")))!;
                    foreach(var total in totals.Where(t=>t.Program==group.Key.ToString()))
                    {
                        var property=typeof(ProgressPaymentCalculation).GetProperty(total.Field)!;
                        var actual=saved.ProgressPayments.Sum(p=>(decimal)property.GetValue(p.Calculation)!);
                        Assert.InRange(System.Math.Abs(actual-total.ExcelSum),0,0.000001m);
                        Assert.Equal(total.ExcelMoney.ToString("N2",CultureInfo.GetCultureInfo("tr-TR")),actual.ToString("N2",CultureInfo.GetCultureInfo("tr-TR")));
                    }
                    var savedOutput=System.Environment.GetEnvironmentVariable("OEHS_PAYMENT_QA_OUTPUT");
                    if(!string.IsNullOrEmpty(savedOutput))
                    {
                        Directory.CreateDirectory(savedOutput);
                        File.WriteAllText(Path.Combine(savedOutput,$"wpf-saved-{group.Key}.json"),JsonSerializer.Serialize(saved,ReleaseAuditEvidenceJsonOptions));
                    }
                }
                finally {directory.Delete(true);}
            }
        });
    }
}
