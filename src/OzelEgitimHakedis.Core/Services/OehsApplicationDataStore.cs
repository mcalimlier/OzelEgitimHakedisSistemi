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
            return OehsApplicationData.Empty;

        try
        {
            return ReadFile(_filePath);
        }
        catch (Exception primaryException) when (
            primaryException is IOException or UnauthorizedAccessException or JsonException)
        {
            if (File.Exists(BackupFilePath))
            {
                try
                {
                    return ReadFile(BackupFilePath);
                }
                catch (Exception backupException) when (
                    backupException is IOException or UnauthorizedAccessException or JsonException)
                {
                    throw new OehsApplicationDataStoreException(
                        "OEHS veri dosyası okunamadı ve güvenli yedek de kullanılamıyor. Veri kaybını önlemek için kaydetme durduruldu.",
                        new AggregateException(primaryException, backupException));
                }
            }

            throw new OehsApplicationDataStoreException(
                "OEHS veri dosyası okunamadı. Veri kaybını önlemek için kaydetme durduruldu.",
                primaryException);
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
