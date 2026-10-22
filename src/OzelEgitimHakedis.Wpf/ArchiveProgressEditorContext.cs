using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Wpf;

public sealed record ArchiveProgressEditorContext(
    ContractDraftData Contract,
    Contractor Contractor,
    RouteData Route,
    DateOnly? PeriodDate)
{
    public static ArchiveProgressEditorContext Create(ProgressPaymentArchive archive) =>
        new(
            archive.Contract,
            archive.Contractor,
            archive.Route,
            archive.ProgressPayment.PeriodDate);
}
