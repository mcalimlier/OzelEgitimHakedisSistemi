using System.Text.Json;
using System.Text.RegularExpressions;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class ProgressPaymentArchiveException : Exception
{
    public ProgressPaymentArchiveException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class ProgressPaymentArchiveStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _folderPath;

    public ProgressPaymentArchiveStore(string? folderPath = null)
    {
        _folderPath = folderPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            "OzelEgitimHakedisSistemi",
            "Arsiv",
            "Hakedisler");
    }

    public IReadOnlyList<FileInfo> List()
    {
        Directory.CreateDirectory(_folderPath);

        return Directory
            .EnumerateFiles(_folderPath, "*.json", SearchOption.TopDirectoryOnly)
            .Where(IsArchiveFile)
            .Select(path => new FileInfo(path))
            .OrderBy(file => file.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static ProgressPaymentArchive Load(string filePath)
    {
        try
        {
            var json = File.ReadAllText(filePath);
            var archive = JsonSerializer.Deserialize<ProgressPaymentArchive>(json, JsonOptions);
            if (archive is null)
                throw new ProgressPaymentArchiveException("Arşiv dosyası boş veya okunamadı.");

            if (archive.Revision < 1)
                throw new ProgressPaymentArchiveException("Arşiv revision numarası geçersiz.");

            if (string.IsNullOrWhiteSpace(archive.SchemaVersion))
                throw new ProgressPaymentArchiveException("Arşiv schema sürümü bulunamadı.");

            if (archive.Contract is null || archive.Contractor is null || archive.Route is null || archive.ProgressPayment is null)
                throw new ProgressPaymentArchiveException("Arşiv gerekli snapshot alanlarını içermiyor.");

            return archive;
        }
        catch (ProgressPaymentArchiveException)
        {
            throw;
        }
        catch (IOException ex)
        {
            throw new ProgressPaymentArchiveException("Arşiv dosyası okunamadı.", ex);
        }
        catch (JsonException ex)
        {
            throw new ProgressPaymentArchiveException("Arşiv JSON biçimi geçersiz.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ProgressPaymentArchiveException("Arşiv dosyasına erişim izni yok.", ex);
        }
    }

    public string SaveRevision(string sourcePath, ProgressPaymentArchive archive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourcePath);
        ArgumentNullException.ThrowIfNull(archive);
        ValidateArchiveIdentity(archive);
        archive = CompleteMissingArchiveIdentity(archive);
        ValidateArchiveIdentity(archive);

        var fullSourcePath = Path.GetFullPath(sourcePath);
        if (!File.Exists(fullSourcePath))
            throw new ProgressPaymentArchiveException("Düzeltilecek mevcut arşiv bulunamadı; yeni revision oluşturulmadı.");

        var sourceArchive = Load(fullSourcePath);
        if (sourceArchive.ProgressPayment.Id != archive.ProgressPayment.Id)
        {
            throw new ProgressPaymentArchiveException(
                "Düzeltme aynı hakediş kimliğini korumalıdır. Kaynak arşiv değiştirilmedi.");
        }

        var folder = Path.GetDirectoryName(fullSourcePath);
        if (string.IsNullOrWhiteSpace(folder))
            throw new ProgressPaymentArchiveException("Arşiv dosya klasörü çözümlenemedi.");

        Directory.CreateDirectory(folder);
        var targetBaseStem = BuildBaseStem(archive);
        var archiveFiles = Directory
            .EnumerateFiles(folder, "*.json", SearchOption.TopDirectoryOnly)
            .Where(IsArchiveFile)
            .ToArray();

        // A changed route/tender/period must not collide with a different payment
        // that already uses the target archive identity. The same logical payment
        // can keep a revision chain while its identity fields are corrected.
        foreach (var existingPath in archiveFiles.Where(file =>
                     Path.GetFileNameWithoutExtension(file).StartsWith(
                         targetBaseStem + "_v",
                         StringComparison.OrdinalIgnoreCase)))
        {
            ProgressPaymentArchive existing;
            try
            {
                existing = Load(existingPath);
            }
            catch (ProgressPaymentArchiveException ex)
            {
                throw new ProgressPaymentArchiveException(
                    "Hedef arşiv kimliğinde okunamayan bir dosya var. Veri kaybını önlemek için yeni revision oluşturulmadı.",
                    ex);
            }

            if (existing.ProgressPayment.Id != archive.ProgressPayment.Id)
            {
                throw new ProgressPaymentArchiveException(
                    "Bu ihale/hat/hakediş/dönem kimliği başka bir arşiv kaydında kullanılıyor. Mevcut revisionlar korunuyor.");
            }
        }

        var maxRevision = sourceArchive.Revision;
        foreach (var existingPath in archiveFiles)
        {
            try
            {
                var existing = Load(existingPath);
                if (existing.ProgressPayment.Id == archive.ProgressPayment.Id)
                    maxRevision = Math.Max(maxRevision, existing.Revision);
            }
            catch (ProgressPaymentArchiveException)
            {
                // Unrelated malformed archives do not get overwritten. Target-identity
                // malformed files have already been rejected by the check above.
            }
        }

        if (sourceArchive.Revision < maxRevision)
        {
            throw new ProgressPaymentArchiveException(
                $"Bu hakedişin daha yeni bir revision'ı (v{maxRevision:00}) var. Eski revision üzerinden düzeltme yapılamaz; en güncel arşiv kaydını çağırın.");
        }

        var nextRevision = checked(maxRevision + 1);
        string targetPath;
        ProgressPaymentArchive revised;
        do
        {
            revised = archive with { Revision = nextRevision };
            targetPath = Path.Combine(folder, BuildFileStem(revised) + ".json");
            if (!File.Exists(targetPath))
                break;

            nextRevision = checked(nextRevision + 1);
        }
        while (true);

        var json = JsonSerializer.Serialize(revised, JsonOptions);
        var tempPath = targetPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, targetPath, overwrite: false);
            return targetPath;
        }
        catch (ProgressPaymentArchiveException)
        {
            throw;
        }
        catch (IOException ex)
        {
            throw new ProgressPaymentArchiveException(
                "Yeni arşiv revision'ı kaydedilemedi; önceki revision korunuyor.",
                ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ProgressPaymentArchiveException(
                "Yeni arşiv revision'ı için yazma izni yok; önceki revision korunuyor.",
                ex);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public string Save(ProgressPaymentArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);
        if (archive.Revision < 1)
            throw new ArgumentOutOfRangeException(nameof(archive), "Revision 1 veya daha büyük olmalıdır.");
        ValidateArchiveIdentity(archive);
        archive = CompleteMissingArchiveIdentity(archive);
        ValidateArchiveIdentity(archive);

        Directory.CreateDirectory(_folderPath);

        var nextRevision = FindNextRevision(archive);
        var normalized = archive with { Revision = nextRevision };
        var targetPath = Path.Combine(_folderPath, BuildFileStem(normalized) + ".json");

        var json = JsonSerializer.Serialize(normalized, JsonOptions);
        var tempPath = targetPath + "." + Guid.NewGuid().ToString("N") + ".tmp";

        try
        {
            File.WriteAllText(tempPath, json);

            try
            {
                File.Move(tempPath, targetPath, overwrite: false);
            }
            catch (IOException)
            {
                // Aynı anda başka bir kayıt v01/v02 oluşturduysa bir sonraki
                // revision numarasına geç; hiçbir eski revision ezilmez.
                nextRevision = FindNextRevision(archive, nextRevision + 1);
                normalized = archive with { Revision = nextRevision };
                targetPath = Path.Combine(_folderPath, BuildFileStem(normalized) + ".json");
                json = JsonSerializer.Serialize(normalized, JsonOptions);
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, targetPath, overwrite: false);
            }

            return targetPath;
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    public static string BuildFileStem(ProgressPaymentArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);

        var tender = SafeFilePart(archive.ProgressPayment.TenderNumber, "Ihale");
        var contractNumber = SafeFilePart(
            string.IsNullOrWhiteSpace(archive.ProgressPayment.ContractNumber)
                ? archive.Contract.ContractNumber
                : archive.ProgressPayment.ContractNumber,
            "Sozlesme");
        var route = SafeFilePart(archive.ProgressPayment.RouteNumber, "Hat");
        var progress = archive.ProgressPayment.ProgressPaymentNumber?.ToString("00", System.Globalization.CultureInfo.InvariantCulture) ?? "Yeni";
        var period = archive.ProgressPayment.PeriodDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "Donemsiz";

        return $"OEHS_{tender}_Soz-{contractNumber}_Hat-{route}_Hakediş-{progress}_{period}_v{archive.Revision:00}";
    }

    private int FindNextRevision(ProgressPaymentArchive archive, int minimumRevision = 1)
    {
        var baseStem = BuildBaseStem(archive);
        var revisions = Directory
            .EnumerateFiles(_folderPath, baseStem + "_v*.json", SearchOption.TopDirectoryOnly)
            .Select(path => ParseRevision(Path.GetFileNameWithoutExtension(path), baseStem))
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToHashSet();

        var next = Math.Max(1, minimumRevision);
        while (revisions.Contains(next))
            next = checked(next + 1);

        return next;
    }

    private static string BuildBaseStem(ProgressPaymentArchive archive)
    {
        var tender = SafeFilePart(archive.ProgressPayment.TenderNumber, "Ihale");
        var contractNumber = SafeFilePart(
            string.IsNullOrWhiteSpace(archive.ProgressPayment.ContractNumber)
                ? archive.Contract.ContractNumber
                : archive.ProgressPayment.ContractNumber,
            "Sozlesme");
        var route = SafeFilePart(archive.ProgressPayment.RouteNumber, "Hat");
        var progress = archive.ProgressPayment.ProgressPaymentNumber?.ToString("00", System.Globalization.CultureInfo.InvariantCulture) ?? "Yeni";
        var period = archive.ProgressPayment.PeriodDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "Donemsiz";
        return $"OEHS_{tender}_Soz-{contractNumber}_Hat-{route}_Hakediş-{progress}_{period}";
    }

    private static int? ParseRevision(string fileName, string baseStem)
    {
        var match = Regex.Match(
            fileName,
            "^" + Regex.Escape(baseStem) + @"_v(?<revision>\d+)$",
            RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        return match.Success && int.TryParse(
            match.Groups["revision"].Value,
            System.Globalization.NumberStyles.None,
            System.Globalization.CultureInfo.InvariantCulture,
            out var revision)
            ? revision
            : null;
    }

    private static ProgressPaymentArchive CompleteMissingArchiveIdentity(ProgressPaymentArchive archive)
    {
        static string FirstPopulated(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

        var tenderNumber = FirstPopulated(
            archive.ProgressPayment.TenderNumber,
            archive.Route.TenderNumber,
            archive.Contract.TenderNumber);
        var contractNumber = FirstPopulated(
            archive.ProgressPayment.ContractNumber,
            archive.Route.ContractNumber,
            archive.Contract.ContractNumber);
        var routeNumber = FirstPopulated(
            archive.ProgressPayment.RouteNumber,
            archive.Route.RouteNumber);
        var contractorIdentity = FirstPopulated(
            archive.ProgressPayment.ContractorTaxOrIdentityNumber,
            archive.Route.ContractorTaxOrIdentityNumber,
            archive.Contractor.TaxOrIdentityNumber);
        var routeName = FirstPopulated(archive.Route.Name, archive.ProgressPayment.RouteName);

        var contract = archive.Contract with
        {
            TenderNumber = FirstPopulated(archive.Contract.TenderNumber, tenderNumber),
            ContractNumber = FirstPopulated(archive.Contract.ContractNumber, contractNumber)
        };
        var contractor = archive.Contractor with
        {
            Name = FirstPopulated(
                archive.Contractor.Name,
                archive.ProgressPayment.ContractorName,
                contract.ContractorName),
            TaxOrIdentityNumber = FirstPopulated(archive.Contractor.TaxOrIdentityNumber, contractorIdentity)
        };
        var route = archive.Route with
        {
            Name = routeName,
            TenderNumber = FirstPopulated(archive.Route.TenderNumber, tenderNumber),
            ContractNumber = FirstPopulated(archive.Route.ContractNumber, contractNumber),
            RouteNumber = FirstPopulated(archive.Route.RouteNumber, routeNumber),
            ContractorTaxOrIdentityNumber = FirstPopulated(
                archive.Route.ContractorTaxOrIdentityNumber,
                contractorIdentity)
        };
        var payment = archive.ProgressPayment with
        {
            TenderNumber = FirstPopulated(archive.ProgressPayment.TenderNumber, tenderNumber),
            ContractNumber = FirstPopulated(archive.ProgressPayment.ContractNumber, contractNumber),
            RouteNumber = FirstPopulated(archive.ProgressPayment.RouteNumber, routeNumber),
            RouteName = FirstPopulated(archive.ProgressPayment.RouteName, routeName),
            ContractorName = FirstPopulated(archive.ProgressPayment.ContractorName, contractor.Name),
            ContractorTaxOrIdentityNumber = FirstPopulated(
                archive.ProgressPayment.ContractorTaxOrIdentityNumber,
                contractorIdentity)
        };
        var previousPayments = (archive.PreviousPayments ?? Array.Empty<ProgressPaymentHistoryData>())
            .Select(previous => previous with
            {
                TenderNumber = FirstPopulated(previous.TenderNumber, tenderNumber),
                ContractNumber = FirstPopulated(previous.ContractNumber, contractNumber),
                RouteNumber = FirstPopulated(previous.RouteNumber, routeNumber),
                ContractorTaxOrIdentityNumber = FirstPopulated(
                    previous.ContractorTaxOrIdentityNumber,
                    contractorIdentity)
            })
            .ToArray();

        return archive with
        {
            Contract = contract,
            Contractor = contractor,
            Route = route,
            ProgressPayment = payment,
            PreviousPayments = previousPayments
        };
    }

    private static void ValidateArchiveIdentity(ProgressPaymentArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);

        EnsureIdentityMatches(
            "İhale numarası",
            archive.Contract.TenderNumber,
            archive.Route.TenderNumber,
            archive.ProgressPayment.TenderNumber);

        EnsureIdentityMatches(
            "Sözleşme numarası",
            archive.Contract.ContractNumber,
            archive.Route.ContractNumber,
            archive.ProgressPayment.ContractNumber);

        EnsureIdentityMatches(
            "Hat numarası",
            archive.Route.RouteNumber,
            archive.ProgressPayment.RouteNumber);

        EnsureIdentityMatches(
            "Yüklenici VKN/TCKN",
            archive.Contractor.TaxOrIdentityNumber,
            archive.Route.ContractorTaxOrIdentityNumber,
            archive.ProgressPayment.ContractorTaxOrIdentityNumber);

        foreach (var previous in archive.PreviousPayments ?? Array.Empty<ProgressPaymentHistoryData>())
        {
            EnsureIdentityMatches(
                "Önceki hakediş ihale numarası",
                archive.ProgressPayment.TenderNumber,
                previous.TenderNumber);
            EnsureIdentityMatches(
                "Önceki hakediş sözleşme numarası",
                archive.ProgressPayment.ContractNumber,
                previous.ContractNumber);
            EnsureIdentityMatches(
                "Önceki hakediş hat numarası",
                archive.ProgressPayment.RouteNumber,
                previous.RouteNumber);
            EnsureIdentityMatches(
                "Önceki hakediş yüklenici VKN/TCKN",
                archive.ProgressPayment.ContractorTaxOrIdentityNumber,
                previous.ContractorTaxOrIdentityNumber);
        }
    }

    private static void EnsureIdentityMatches(string label, params string?[] values)
    {
        var populated = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!.Trim())
            .ToArray();

        if (populated.Length <= 1)
            return;

        if (label.Contains("VKN/TCKN", StringComparison.Ordinal))
        {
            var normalized = populated
                .Select(OehsIdentityService.NormalizeIdentity)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            if (normalized.Length <= 1)
                return;
        }
        else
        {
            if (populated.Distinct(StringComparer.OrdinalIgnoreCase).Count() <= 1)
                return;
        }

        throw new ProgressPaymentArchiveException(
            $"Arşiv kaydı tutarsız: {label} alanları birbiriyle uyuşmuyor. Yeni revision kaydedilmedi.");
    }

    private static bool IsArchiveFile(string path)
    {
        var name = Path.GetFileName(path);
        return name.StartsWith("OEHS_", StringComparison.OrdinalIgnoreCase)
            && Regex.IsMatch(name, @"_v\d+\.json$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    private static string SafeFilePart(string? value, string fallback) =>
        OehsFileNameService.SanitizeFileName(value, fallback);
}
