using System.Text.Json;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class OehsApplicationDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly string _filePath;

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
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<OehsApplicationData>(json, JsonOptions)
                   ?? OehsApplicationData.Empty;
        }
        catch (IOException)
        {
            return OehsApplicationData.Empty;
        }
        catch (JsonException)
        {
            return OehsApplicationData.Empty;
        }
    }

    public void Save(OehsApplicationData data)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        var temporaryPath = _filePath + ".tmp";
        var json = JsonSerializer.Serialize(data, JsonOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, _filePath, true);
    }
}