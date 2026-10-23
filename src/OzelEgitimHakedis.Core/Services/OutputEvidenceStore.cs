using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record OutputEvidence(string ContextHash, IReadOnlyDictionary<string, string> Files);
public static class OutputEvidenceStore
{
    public static string HashContext(string context) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(context)));
    public static string FileHash(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }
    public static string ResolveInside(string root, string relative)
    {
        if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.Contains(':', StringComparison.Ordinal))
            throw new InvalidDataException("Geçersiz göreli dosya yolu.");
        var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var path = Path.GetFullPath(Path.Combine(fullRoot, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Dosya paket kökü dışında.");
        return path;
    }
    public static void Save(string root, Guid paymentId, string contextHash, IReadOnlyList<string> pdfPaths, TransportationProgramType? program = null)
    {
        if (pdfPaths.Count != 4 || pdfPaths.Distinct(StringComparer.OrdinalIgnoreCase).Count() != 4)
            throw new ArgumentException("Dört ayrı PDF dosyası gereklidir.", nameof(pdfPaths));
        var files = pdfPaths.ToDictionary(p => Path.GetRelativePath(root, p).Replace('\\','/'), p =>
        {
            _ = ResolveInside(root, Path.GetRelativePath(root, p));
            return FileHash(p);
        }, StringComparer.OrdinalIgnoreCase);
        var path = EvidencePath(root, paymentId, program);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(new OutputEvidence(contextHash, files)));
        File.Move(path + ".tmp", path, true);
    }
    public static bool IsCurrent(string root, Guid paymentId, string contextHash, TransportationProgramType? program = null)
    {
        try
        {
            var path = EvidencePath(root, paymentId, program);
            if (!File.Exists(path)) return false;
            var evidence = JsonSerializer.Deserialize<OutputEvidence>(File.ReadAllText(path));
            return evidence is not null && evidence.ContextHash == contextHash && evidence.Files.Count == 4 &&
                evidence.Files.All(f => File.Exists(ResolveInside(root, f.Key)) && FileHash(ResolveInside(root, f.Key)) == f.Value);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException) { return false; }
    }
    private static string EvidencePath(string root, Guid id, TransportationProgramType? program) =>
        Path.Combine(root, ".oehs-output", program?.ToString() ?? "", id.ToString("N") + ".json");
}
