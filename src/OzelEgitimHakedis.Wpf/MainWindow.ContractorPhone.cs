using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private static IReadOnlyList<PaymentDifference> CompareContractorFields(Contractor original, Contractor updated) =>
        PaymentOperationsService.CompareFields(original with {Phone=NormalizePhoneNumber(original.Phone)},
            updated with {Phone=NormalizePhoneNumber(updated.Phone)},"Firma");
}
