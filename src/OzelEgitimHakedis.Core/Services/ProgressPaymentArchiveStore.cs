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

    public string Replace(string filePath, ProgressPaymentArchive archive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(archive);

        if (archive.Revision < 1)
            throw new ArgumentOutOfRangeException(nameof(archive), "Revision 1 veya daha büyük olmalıdır.");

        var fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
            throw new ProgressPaymentArchiveException("Mevcut arşiv dosyası bulunamadı; yeni arşiv oluşturulmadı.");

        var folder = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(folder))
            throw new ProgressPaymentArchiveException("Arşiv dosya klasörü çözümlenemedi.");

        Directory.CreateDirectory(folder);

        // Düzeltme aynı revision'a aittir. Kimlik alanları (özellikle Hat No)
        // değiştiğinde dosya adı da aynı revision korunarak yeni kimliğe taşınır;
        // v02/v03 gibi yeni revision oluşturulmaz.
        var targetPath = Path.Combine(folder, BuildFileStem(archive) + ".json");
        var samePath = string.Equals(fullPath, targetPath, StringComparison.OrdinalIgnoreCase);

        if (!samePath && File.Exists(targetPath))
        {
            throw new ProgressPaymentArchiveException(
                "Düzeltme sonucu oluşacak arşiv dosyası zaten mevcut. Mevcut arşiv değiştirilmedi ve yeni revision oluşturulmadı.");
        }

        var json = JsonSerializer.Serialize(archive, JsonOptions);
        var tempPath = Path.Combine(
            folder,
            Path.GetFileName(targetPath) + "." + Guid.NewGuid().ToString("N") + ".tmp");

        try
        {
            File.WriteAllText(tempPath, json);

            if (samePath)
            {
                File.Move(tempPath, fullPath, overwrite: true);
                return fullPath;
            }

            File.Move(tempPath, targetPath, overwrite: false);

            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                // Eski dosya silinemiyorsa yeni hedefi geri al; böylece düzeltme
                // tamamlanmış görünmeden önce eski kayıt korunur.
                if (File.Exists(targetPath))
                    File.Delete(targetPath);
                throw;
            }

            return targetPath;
        }
        catch (ProgressPaymentArchiveException)
        {
            throw;
        }
        catch (IOException ex)
        {
            throw new ProgressPaymentArchiveException("Mevcut arşiv dosyası güncellenemedi.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new ProgressPaymentArchiveException("Mevcut arşiv dosyasına yazma izni yok.", ex);
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
        var route = SafeFilePart(archive.ProgressPayment.RouteNumber, "Hat");
        var progress = archive.ProgressPayment.ProgressPaymentNumber?.ToString("00", System.Globalization.CultureInfo.InvariantCulture) ?? "Yeni";
        var period = archive.ProgressPayment.PeriodDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "Donemsiz";

        return $"OEHS_{tender}_Hat-{route}_Hakediş-{progress}_{period}_v{archive.Revision:00}";
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
        var route = SafeFilePart(archive.ProgressPayment.RouteNumber, "Hat");
        var progress = archive.ProgressPayment.ProgressPaymentNumber?.ToString("00", System.Globalization.CultureInfo.InvariantCulture) ?? "Yeni";
        var period = archive.ProgressPayment.PeriodDate?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) ?? "Donemsiz";
        return $"OEHS_{tender}_Hat-{route}_Hakediş-{progress}_{period}";
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

    private static bool IsArchiveFile(string path)
    {
        var name = Path.GetFileName(path);
        return name.StartsWith("OEHS_", StringComparison.OrdinalIgnoreCase)
            && Regex.IsMatch(name, @"_v\d+\.json$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
    }

    private static string SafeFilePart(string? value, string fallback) =>
        OehsFileNameService.SanitizeFileName(value, fallback);
}
