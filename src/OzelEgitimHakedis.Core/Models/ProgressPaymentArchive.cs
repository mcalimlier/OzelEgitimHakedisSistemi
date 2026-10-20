namespace OzelEgitimHakedis.Core.Models;

public sealed record ProgressPaymentArchive(
    string SchemaVersion,
    int Revision,
    DateTime ArchivedAt,
    string Source,
    ContractDraftData Contract,
    Contractor Contractor,
    RouteData Route,
    ProgressPaymentHistoryData ProgressPayment,
    IReadOnlyList<ProgressPaymentHistoryData> PreviousPayments,
    string PdfFileName,
    IReadOnlyList<string> Documents);
