using System.Text.Json;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class OehsApplicationDataStoreException : Exception
{
    public OehsApplicationDataStoreException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public sealed class OehsApplicationDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;
    private string BackupFilePath => _filePath + ".bak";

    public OehsApplicationDataStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "OEHS",
            "oehs-data.json");
    }

    public OehsApplicationData Load()
    {
        if (!File.Exists(_filePath))
        {
            if (!File.Exists(BackupFilePath))
                return OehsApplicationData.Empty;

            return RestoreAndReadBackup();
        }

        try
        {
            return ReadFile(_filePath);
        }
        catch (Exception primaryException) when (
            primaryException is IOException or UnauthorizedAccessException or JsonException)
        {
            if (File.Exists(BackupFilePath))
                return RestoreAndReadBackup(primaryException);

            throw new OehsApplicationDataStoreException(
                "OEHS veri dosyası okunamadı. Veri kaybını önlemek için kaydetme durduruldu.",
                primaryException);
        }
    }

    private OehsApplicationData RestoreAndReadBackup(Exception? primaryException = null)
    {
        try
        {
            var backupData = ReadFile(BackupFilePath);

            // Yedek doğrulandıktan sonra ana dosyayı da geri getiriyoruz.
            // Böylece sonraki kayıt işlemi bozuk ana dosyayı tekrar .bak üzerine yazamaz.
            File.Copy(BackupFilePath, _filePath, overwrite: true);
            return backupData;
        }
        catch (Exception backupException) when (
            backupException is IOException or UnauthorizedAccessException or JsonException)
        {
            var inner = primaryException is null
                ? backupException
                : new AggregateException(primaryException, backupException);

            throw new OehsApplicationDataStoreException(
                "OEHS veri dosyası okunamadı ve güvenli yedek de kullanılamıyor. Veri kaybını önlemek için kaydetme durduruldu.",
                inner);
        }
    }

    public void Save(OehsApplicationData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var temporaryPath = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(data, JsonOptions);

        try
        {
            if (File.Exists(_filePath))
                File.Copy(_filePath, BackupFilePath, overwrite: true);

            File.WriteAllText(temporaryPath, json);
            File.Move(temporaryPath, _filePath, overwrite: true);
        }
        catch (IOException ex)
        {
            throw new OehsApplicationDataStoreException("OEHS verisi güvenli biçimde kaydedilemedi.", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new OehsApplicationDataStoreException("OEHS veri dosyasına yazma izni yok.", ex);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static OehsApplicationData ReadFile(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<OehsApplicationData>(json, JsonOptions)
               ?? throw new JsonException("OEHS veri dosyası boş.");
    }
}
