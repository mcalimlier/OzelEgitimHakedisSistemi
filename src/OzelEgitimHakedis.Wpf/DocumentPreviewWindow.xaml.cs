using System.Windows;
using System.Windows.Documents;

namespace OzelEgitimHakedis.Wpf;

public partial class DocumentPreviewWindow : Window
{
    public DocumentPreviewWindow(FixedDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        InitializeComponent();
        TextSharpnessService.Apply(this);
        Viewer.Document = document;
        Loaded += (_, _) => FitToOwner();
    }

    private void FitToOwner()
    {
        var area = SystemParameters.WorkArea;
        var availableWidth = Math.Max(1d, (Owner?.ActualWidth > 0 ? Owner.ActualWidth : area.Width) - 40d);
        var availableHeight = Math.Max(1d, (Owner?.ActualHeight > 0 ? Owner.ActualHeight : area.Height) - 40d);
        MinWidth = Math.Min(MinWidth, availableWidth);
        MinHeight = Math.Min(MinHeight, availableHeight);
        Width = Math.Min(Width, availableWidth);
        Height = Math.Min(Height, availableHeight);
        Left = (Owner?.Left ?? area.Left) + ((Owner?.ActualWidth > 0 ? Owner.ActualWidth : area.Width) - Width) / 2d;
        Top = (Owner?.Top ?? area.Top) + ((Owner?.ActualHeight > 0 ? Owner.ActualHeight : area.Height) - Height) / 2d;
        Viewer.FitToWidth();
    }
}
