using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;
public sealed class OutputEvidenceTests
{
    [Fact] public void Evidence_RequiresFourUnchangedFilesAndSameContextAndIsPortable()
    {
        var root = Directory.CreateTempSubdirectory("oehs-evidence-");
        try
        {
            var paths = Enumerable.Range(1,4).Select(i => Path.Combine(root.FullName, i + ".pdf")).ToArray();
            foreach (var p in paths) File.WriteAllText(p, "%PDF-test");
            var id = Guid.NewGuid();
            OutputEvidenceStore.Save(root.FullName, id, "context-a", paths);
            Assert.True(OutputEvidenceStore.IsCurrent(root.FullName, id, "context-a"));
            var portable=Directory.CreateDirectory(Path.Combine(root.FullName,"moved"));
            foreach(var file in Directory.GetFiles(root.FullName)) File.Copy(file,Path.Combine(portable.FullName,Path.GetFileName(file)));
            Directory.CreateDirectory(Path.Combine(portable.FullName,".oehs-output"));
            foreach(var file in Directory.GetFiles(Path.Combine(root.FullName,".oehs-output")))
                File.Copy(file,Path.Combine(portable.FullName,".oehs-output",Path.GetFileName(file)));
            Assert.True(OutputEvidenceStore.IsCurrent(portable.FullName,id,"context-a"));
            Assert.False(OutputEvidenceStore.IsCurrent(root.FullName, id, "context-b"));
            File.AppendAllText(paths[0], "changed");
            Assert.False(OutputEvidenceStore.IsCurrent(root.FullName, id, "context-a"));
            Assert.Throws<ArgumentException>(() => OutputEvidenceStore.Save(root.FullName, id, "context-a", paths.Take(3).ToArray()));
        }
        finally { root.Delete(true); }
    }
}
