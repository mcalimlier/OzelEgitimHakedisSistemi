using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public enum ProgressCorrectionTarget
{
    Tender,
    Contractor,
    Route,
    ProgressPayment,
    Cancel
}

public sealed record ProgressCorrectionTargetSelection(
    ProgressCorrectionTarget Target,
    string? TenderNumber,
    string? ContractorTaxOrIdentityNumber,
    string? RouteNumber,
    Guid? ProgressPaymentId);

public static class ProgressCorrectionTargetService
{
    public static ProgressCorrectionTargetSelection Select(
        ProgressCorrectionTarget target,
        ProgressPaymentHistoryData item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return target switch
        {
            ProgressCorrectionTarget.Tender => new(
                target,
                item.TenderNumber,
                null,
                null,
                null),

            ProgressCorrectionTarget.Contractor => new(
                target,
                item.TenderNumber,
                OehsIdentityService.NormalizeIdentity(item.ContractorTaxOrIdentityNumber ?? string.Empty),
                null,
                null),

            ProgressCorrectionTarget.Route => new(
                target,
                item.TenderNumber,
                OehsIdentityService.NormalizeIdentity(item.ContractorTaxOrIdentityNumber ?? string.Empty),
                item.RouteNumber,
                null),

            ProgressCorrectionTarget.ProgressPayment => new(
                target,
                item.TenderNumber,
                OehsIdentityService.NormalizeIdentity(item.ContractorTaxOrIdentityNumber ?? string.Empty),
                item.RouteNumber,
                item.Id),

            ProgressCorrectionTarget.Cancel => new(
                target,
                null,
                null,
                null,
                null),

            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Desteklenmeyen düzeltme hedefi.")
        };
    }
}
