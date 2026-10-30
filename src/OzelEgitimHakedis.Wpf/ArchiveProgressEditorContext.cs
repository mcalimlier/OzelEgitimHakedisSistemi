using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public sealed record ArchiveProgressEditorContext(
    ContractDraftData Contract,
    Contractor Contractor,
    RouteData Route,
    DateOnly? PeriodDate)
{
    public static ArchiveProgressEditorContext Create(ProgressPaymentArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        if (archive.Contract is null)
            throw new ProgressPaymentArchiveException("Arşiv sözleşme bilgisi (Contract) eksik.");
        if (archive.Contractor is null)
            throw new ProgressPaymentArchiveException("Arşiv yüklenici bilgisi (Contractor) eksik.");
        if (archive.Route is null)
            throw new ProgressPaymentArchiveException("Arşiv hat bilgisi (Route) eksik.");
        if (archive.ProgressPayment is null)
            throw new ProgressPaymentArchiveException("Arşiv hakediş bilgisi (ProgressPayment) eksik.");
        if (string.IsNullOrWhiteSpace(archive.Contract.TenderNumber))
            throw new ProgressPaymentArchiveException("Arşiv ihale kayıt numarası (Contract.TenderNumber) eksik.");
        if (string.IsNullOrWhiteSpace(archive.Contract.ContractNumber))
            throw new ProgressPaymentArchiveException("Arşiv sözleşme numarası (Contract.ContractNumber) eksik.");
        if (string.IsNullOrWhiteSpace(archive.Route.RouteNumber))
            throw new ProgressPaymentArchiveException("Arşiv hat numarası (Route.RouteNumber) eksik.");
        if (!archive.ProgressPayment.PeriodDate.HasValue)
            throw new ProgressPaymentArchiveException("Arşiv hakediş dönemi (ProgressPayment.PeriodDate) eksik.");

        return new(
            archive.Contract,
            archive.Contractor,
            archive.Route,
            archive.ProgressPayment.PeriodDate);
    }
}
