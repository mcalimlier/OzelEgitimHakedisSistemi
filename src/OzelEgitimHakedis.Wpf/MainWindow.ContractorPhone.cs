using System.Windows.Controls;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private bool _settingContractorPhone;
    private string? _acceptedContractorPhone;

    private static bool IsPhoneEntry(string text) => text.Length==0 ||
        text.Length<=10 && text[0]=='5' && text.All(c=>c is >= '0' and <= '9');

    private void ContractorPhoneTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if(_settingContractorPhone) return;
        var box=(TextBox)sender;
        if(IsPhoneEntry(box.Text))
        {
            _acceptedContractorPhone=box.Text;
            return;
        }
        var caret=box.CaretIndex;
        _settingContractorPhone=true;
        try
        {
            box.Text=_acceptedContractorPhone??string.Empty;
            box.CaretIndex=Math.Min(caret,box.Text.Length);
        }
        finally {_settingContractorPhone=false;}
    }

    private void SetContractorPhoneText(string? storedPhone)
    {
        var canonical=NormalizePhoneNumber(storedPhone);
        _settingContractorPhone=true;
        try
        {
            // Loading legacy records changes presentation only, never their stored data.
            ContractorPhoneTextBox.Text=canonical.Length>0?canonical[1..]:string.Empty;
            _acceptedContractorPhone=ContractorPhoneTextBox.Text;
        }
        finally {_settingContractorPhone=false;}
    }

    private bool ContractorPhoneDiffers(string? storedPhone) =>
        !string.Equals(NormalizePhoneNumber(ContractorPhoneTextBox.Text),NormalizePhoneNumber(storedPhone),StringComparison.Ordinal);

    private bool ValidateContractorPhone()
    {
        var text=ContractorPhoneTextBox.Text;
        if(IsPhoneEntry(text) && (text.Length==0 || text.Length==10)) return true;
        ContractorStatusTextBlock.Text="Telefonu baştaki 0 olmadan, 5 ile başlayan 10 hane olarak girin veya boş bırakın. Firma kaydedilmedi.";
        return false;
    }
}
