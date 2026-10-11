using System.Windows;
using System.Windows.Documents;

namespace OzelEgitimHakedis.Wpf;

public partial class DocumentPreviewWindow : Window
{
    public DocumentPreviewWindow(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        InitializeComponent();
        TextSharpnessService.Apply(this);
        Viewer.Document = document;
    }
}
