using System.IO.Compression;
using System.Text.Json;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;
public sealed class OehsBackupTests
{
    [Fact] public void Backup_RejectsSemanticallyInvalidCalculationRates()
    {
        var root=Directory.CreateTempSubdirectory("oehs-invalid-rates-");
        try
        {
            var data=Path.Combine(root.FullName,"data");
            new OehsProgramScopedApplicationDataStore(data).Save(PaymentOperationsTests.Data() with
                {CalculationConstants=OzelEgitimHakedis.Core.Models.ProgressPaymentCalculationConstants.Defaults with {TransportVatRate=2m}});
            Assert.Throws<InvalidDataException>(()=>new OehsBackupService(data,Path.Combine(root.FullName,"outputs"))
                .Create(Path.Combine(root.FullName,"backup.zip")));
        }
        finally {root.Delete(true);}
    }
    [Fact] public void FullBackup_RestoresToAnotherComputerAndPreservesAllFourOutputs()
    {
        var root = Directory.CreateTempSubdirectory("oehs-backup-test-");
        try
        {
            var data = Path.Combine(root.FullName,"source-data"); var output = Path.Combine(root.FullName,"source-output");
            Directory.CreateDirectory(output);
            new OehsProgramScopedApplicationDataStore(data).Save(PaymentOperationsTests.Data(PaymentOperationsTests.Payment));
            File.WriteAllText(Path.Combine(data,".fresh-start-version"),"current");
            for(var i=0;i<4;i++) File.WriteAllText(Path.Combine(output,i+".pdf"),"%PDF");
            File.WriteAllText(Path.Combine(data,"ignored.tmp"),"partial");
            var path = Path.Combine(root.FullName,"backup.zip");
            new OehsBackupService(data,output).Create(path);
            var destination = new OehsBackupService(Path.Combine(root.FullName,"destination-data"),Path.Combine(root.FullName,"destination-output"));
            Assert.Equal(4,OehsBackupService.Inspect(path).PdfCount);
            var recovery = destination.Restore(path);
            Assert.True(File.Exists(recovery));
            Assert.Single(new OehsProgramScopedApplicationDataStore(Path.Combine(root.FullName,"destination-data")).Load().Routes);
            Assert.False(File.Exists(Path.Combine(root.FullName,"destination-data","ignored.tmp")));
            Assert.Equal(4,Directory.GetFiles(Path.Combine(root.FullName,"destination-output"),"*.pdf").Length);
        }
        finally { root.Delete(true); }
    }
    [Fact]
    public void Inspect_RejectsUnsafeZipEntries()
    {
        foreach (var entry in new[] { "../escape.txt", "data/../../escape.txt", "C:/escape.txt" })
        {
        var root=Directory.CreateTempSubdirectory("oehs-unsafe-");
        try
        {
            var path=Path.Combine(root.FullName,"bad.zip");
            using(var zip=ZipFile.Open(path,ZipArchiveMode.Create)) { using var writer=new StreamWriter(zip.CreateEntry(entry).Open()); writer.Write("bad"); }
            Assert.ThrowsAny<InvalidDataException>(()=>OehsBackupService.Inspect(path));
        }
        finally {root.Delete(true);}
        }
    }
    [Fact] public void Restore_RollsBackBothRootsWhenSecondReplacementFails()
    {
        var root=Directory.CreateTempSubdirectory("oehs-rollback-");
        try
        {
            var data=Path.Combine(root.FullName,"data"); var output=Path.Combine(root.FullName,"output");
            Directory.CreateDirectory(data); Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(data,"official-document-settings.json"),"{}");
            File.WriteAllText(Path.Combine(output,"old.pdf"),"old");
            var service=new OehsBackupService(data,output); var path=Path.Combine(root.FullName,"backup.zip"); service.Create(path);
            File.WriteAllText(Path.Combine(output,"old.pdf"),"current");
            Assert.Throws<IOException>(()=>service.Restore(path, key=> {if(key=="outputs") throw new IOException("injected");}));
            Assert.Equal("current",File.ReadAllText(Path.Combine(output,"old.pdf")));
            Assert.Equal("{}",File.ReadAllText(Path.Combine(data,"official-document-settings.json")));
        }
        finally {root.Delete(true);}
    }
    [Fact] public void Inspect_RejectsTamperedContent()
    {
        var root=Directory.CreateTempSubdirectory("oehs-corrupt-");
        try
        {
            var data=Path.Combine(root.FullName,"d"); Directory.CreateDirectory(data);
            File.WriteAllText(Path.Combine(data,"official-document-settings.json"),"{}");
            var service=new OehsBackupService(data,Path.Combine(root.FullName,"o")); var path=Path.Combine(root.FullName,"backup.zip"); service.Create(path);
            using(var zip=ZipFile.Open(path,ZipArchiveMode.Update)) { var e=zip.GetEntry("data/official-document-settings.json")!; e.Delete(); using var w=new StreamWriter(zip.CreateEntry("data/official-document-settings.json").Open()); w.Write("{bad}"); }
            Assert.Throws<InvalidDataException>(()=>OehsBackupService.Inspect(path));
        }
        finally {root.Delete(true);}
    }
}
