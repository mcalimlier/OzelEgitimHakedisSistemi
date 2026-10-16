using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

/// <summary>
/// Persists each transportation program in its own JSON file. Existing single-file
/// installations are migrated on first load; the legacy file is retained as a
/// recovery copy until the user chooses to remove it manually.
/// </summary>
public sealed class OehsProgramScopedApplicationDataStore
{
    private readonly string _root;
    private readonly string _legacyPath;
    private readonly string _specialPath;
    private readonly string _basicPath;

    public OehsProgramScopedApplicationDataStore(string? dataRoot = null)
    {
        _root = dataRoot ?? OehsRuntimePaths.DataRoot;
        _legacyPath = Path.Combine(_root, "oehs-data.json");
        _specialPath = Path.Combine(_root, "oehs-special-education.json");
        _basicPath = Path.Combine(_root, "oehs-basic-secondary.json");
    }

    public OehsApplicationData Load()
    {
        if (ExistsOrBackup(_specialPath) || ExistsOrBackup(_basicPath))
        {
            var special = new OehsApplicationDataStore(_specialPath).Load();
            var basic = new OehsApplicationDataStore(_basicPath).Load();
            return Merge(special, basic);
        }

        if (!ExistsOrBackup(_legacyPath))
            return OehsApplicationData.Empty;

        var legacy = NormalizeLegacyData(new OehsApplicationDataStore(_legacyPath).Load());
        Save(legacy);
        return legacy;
    }

    public void Save(OehsApplicationData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Directory.CreateDirectory(_root);

        var contracts = (data.Contracts ?? Array.Empty<ContractDraftData>()).ToList();
        if (data.Contract is not null && contracts.All(x =>
                !string.Equals(
                    OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber, x.ProgramType),
                    OehsIdentityService.BuildContractKey(data.Contract.TenderNumber, data.Contract.ContractNumber, data.Contract.ProgramType),
                    StringComparison.Ordinal)))
        {
            contracts.Add(data.Contract);
        }

        var special = CreatePartition(data, contracts, TransportationProgramType.SpecialEducation);
        var basic = CreatePartition(data, contracts, TransportationProgramType.BasicOrSecondary);
        var stagedSpecial = _specialPath + ".pending";
        var stagedBasic = _basicPath + ".pending";

        Cleanup(stagedSpecial);
        Cleanup(stagedBasic);

        try
        {
            new OehsApplicationDataStore(stagedSpecial).Save(special);
            new OehsApplicationDataStore(stagedBasic).Save(basic);
            _ = new OehsApplicationDataStore(stagedSpecial).Load();
            _ = new OehsApplicationDataStore(stagedBasic).Load();
            CommitPair(stagedSpecial, stagedBasic);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OehsApplicationDataStoreException)
        {
            throw new OehsApplicationDataStoreException(
                "Özel Eğitim ve Temel/Ortaöğretim verileri güvenli biçimde kaydedilemedi; önceki program dosyaları korunmaya çalışıldı.",
                ex);
        }
        finally
        {
            Cleanup(stagedSpecial);
            Cleanup(stagedBasic);
            Cleanup(stagedSpecial + ".bak");
            Cleanup(stagedBasic + ".bak");
        }
    }

    private OehsApplicationData CreatePartition(
        OehsApplicationData source,
        IReadOnlyList<ContractDraftData> contracts,
        TransportationProgramType programType)
    {
        var scopedContracts = contracts.Where(x => x.ProgramType == programType).ToArray();
        var selectedContract = source.Contract?.ProgramType == programType
            ? source.Contract
            : scopedContracts.FirstOrDefault();

        return new OehsApplicationData(
            selectedContract,
            source.Routes.Where(x => x.ProgramType == programType).ToArray(),
            source.Contractors.Where(x => x.ProgramType == programType).ToArray(),
            (source.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>())
                .Where(x => x.Input.ProgramType == programType).ToArray(),
            scopedContracts,
            source.CalculationConstants,
            source.CalculationConstantsHistory ?? Array.Empty<ProgressPaymentCalculationConstantsChange>());
    }

    private void CommitPair(string stagedSpecial, string stagedBasic)
    {
        var rollbackSpecial = _specialPath + ".rollback";
        var rollbackBasic = _basicPath + ".rollback";
        var hadSpecial = File.Exists(_specialPath);
        var hadBasic = File.Exists(_basicPath);

        Cleanup(rollbackSpecial);
        Cleanup(rollbackBasic);

        try
        {
            if (hadSpecial)
            {
                File.Copy(_specialPath, rollbackSpecial, overwrite: true);
                File.Copy(_specialPath, _specialPath + ".bak", overwrite: true);
            }

            if (hadBasic)
            {
                File.Copy(_basicPath, rollbackBasic, overwrite: true);
                File.Copy(_basicPath, _basicPath + ".bak", overwrite: true);
            }

            File.Move(stagedSpecial, _specialPath, overwrite: true);
            File.Move(stagedBasic, _basicPath, overwrite: true);
        }
        catch
        {
            Restore(_specialPath, rollbackSpecial, hadSpecial);
            Restore(_basicPath, rollbackBasic, hadBasic);
            throw;
        }
        finally
        {
            Cleanup(rollbackSpecial);
            Cleanup(rollbackBasic);
        }
    }

    private static void Restore(string path, string rollbackPath, bool existed)
    {
        if (existed && File.Exists(rollbackPath))
            File.Copy(rollbackPath, path, overwrite: true);
        else if (!existed && File.Exists(path))
            File.Delete(path);
    }

    private static OehsApplicationData Merge(OehsApplicationData special, OehsApplicationData basic)
    {
        var contracts = (special.Contracts ?? Array.Empty<ContractDraftData>())
            .Concat(basic.Contracts ?? Array.Empty<ContractDraftData>())
            .DistinctBy(x => OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber, x.ProgramType))
            .ToArray();
        var routes = special.Routes.Concat(basic.Routes)
            .DistinctBy(x => OehsIdentityService.BuildRouteKey(
                x.TenderNumber, x.RouteNumber, x.ContractorTaxOrIdentityNumber, x.ContractNumber, x.ProgramType))
            .ToArray();
        var contractors = special.Contractors.Concat(basic.Contractors)
            .DistinctBy(x => $"{x.ProgramType}|{OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber)}")
            .ToArray();
        var payments = (special.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>())
            .Concat(basic.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>())
            .DistinctBy(x => $"{x.Id:N}|{x.Input.ProgramType}")
            .ToArray();

        return new OehsApplicationData(
            special.Contract ?? basic.Contract,
            routes,
            contractors,
            payments,
            contracts,
            special.CalculationConstants ?? basic.CalculationConstants,
            (special.CalculationConstantsHistory ?? Array.Empty<ProgressPaymentCalculationConstantsChange>()).Count > 0
                ? special.CalculationConstantsHistory
                : basic.CalculationConstantsHistory);
    }

    private static OehsApplicationData NormalizeLegacyData(OehsApplicationData data)
    {
        var contracts = (data.Contracts ?? Array.Empty<ContractDraftData>()).ToList();
        if (data.Contract is not null && contracts.All(x =>
                !string.Equals(
                    OehsIdentityService.BuildContractKey(x.TenderNumber, x.ContractNumber, x.ProgramType),
                    OehsIdentityService.BuildContractKey(data.Contract.TenderNumber, data.Contract.ContractNumber, data.Contract.ProgramType),
                    StringComparison.Ordinal)))
            contracts.Add(data.Contract);

        var routes = (data.Routes ?? Array.Empty<RouteData>()).Select(route =>
        {
            var identity = OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber);
            var candidates = contracts.Where(contract =>
                string.Equals(contract.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(identity)
                    || string.Equals(
                        OehsIdentityService.NormalizeIdentity(contract.ContractorTaxOrIdentityNumber),
                        identity, StringComparison.Ordinal))
                && (string.IsNullOrWhiteSpace(route.ContractNumber)
                    || string.Equals(contract.ContractNumber, route.ContractNumber, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            return candidates.Length == 1
                ? route with { ProgramType = candidates[0].ProgramType, ContractNumber = candidates[0].ContractNumber }
                : route;
        }).ToArray();

        var payments = (data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>()).Select(payment =>
        {
            var identity = OehsIdentityService.NormalizeIdentity(payment.ContractorTaxOrIdentityNumber);
            var candidates = routes.Where(route =>
                string.Equals(route.TenderNumber, payment.TenderNumber, StringComparison.OrdinalIgnoreCase)
                && string.Equals(route.RouteNumber, payment.RouteNumber, StringComparison.OrdinalIgnoreCase)
                && (string.IsNullOrWhiteSpace(identity)
                    || string.Equals(OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber), identity, StringComparison.Ordinal))
                && (string.IsNullOrWhiteSpace(payment.ContractNumber)
                    || string.Equals(route.ContractNumber, payment.ContractNumber, StringComparison.OrdinalIgnoreCase)))
                .ToArray();

            if (candidates.Length != 1)
                return payment;

            var route = candidates[0];
            return payment with
            {
                Input = payment.Input with { ProgramType = route.ProgramType },
                ContractNumber = string.IsNullOrWhiteSpace(payment.ContractNumber) ? route.ContractNumber : payment.ContractNumber
            };
        }).ToArray();

        var contractors = (data.Contractors ?? Array.Empty<Contractor>())
            .SelectMany(contractor =>
            {
                var identity = OehsIdentityService.NormalizeIdentity(contractor.TaxOrIdentityNumber);
                var linkedPrograms = contracts
                    .Where(x => string.Equals(OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber), identity, StringComparison.Ordinal))
                    .Select(x => x.ProgramType)
                    .Concat(routes.Where(x => string.Equals(OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber), identity, StringComparison.Ordinal))
                        .Select(x => x.ProgramType))
                    .Distinct()
                    .ToArray();
                return (linkedPrograms.Length == 0 ? new[] { contractor.ProgramType } : linkedPrograms)
                    .Select(program => contractor with { ProgramType = program });
            })
            .DistinctBy(x => $"{x.ProgramType}|{OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber)}")
            .ToArray();

        return new OehsApplicationData(
            data.Contract,
            routes,
            contractors,
            payments,
            contracts,
            data.CalculationConstants,
            data.CalculationConstantsHistory);
    }

    private static bool ExistsOrBackup(string path) =>
        File.Exists(path) || File.Exists(path + ".bak");

    private static void Cleanup(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A stale staging file is recoverable and must not hide the primary error.
        }
    }
}