using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class HybridTableExcelExportTests
{
    [Fact]
    public void TableExport_ShouldWriteExactHeadersTypedDecimalsAndLiteralText()
    {
        var directory=Directory.CreateTempSubdirectory("oehs-grid-xlsx-");
        try
        {
            var target=Path.Combine(directory.FullName,"selected-routes.xlsx");
            var method=typeof(ExcelDocumentExporter).GetMethod("SaveTable",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
            Assert.NotNull(method);
            var headers=new[]{"Hattın Adı","Rehber Personel Günlük Fiyat","Döneme Esas Taşıma Hakediş Gün Sayısı (Döneme Esas THGS)","Asgari Ücret Desteği Kullan"};
            System.Collections.Generic.IReadOnlyList<object?>[] rows={new object?[]{"=1+1 & <hat>",1234.5678m,18.5m,true}};
            method!.Invoke(null,new object[]{target,headers,rows});
            using var zip=ZipFile.OpenRead(target);
            Assert.NotNull(zip.GetEntry("[Content_Types].xml"));
            using var stream=zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            var xml=XDocument.Load(stream);
            XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            var records=xml.Descendants(ns+"row").ToArray();
            Assert.Equal(2,records.Length);
            Assert.Equal(headers,records[0].Descendants(ns+"t").Select(e=>e.Value).ToArray());
            var cells=records[1].Elements(ns+"c").ToArray();
            Assert.Equal("inlineStr",(string?)cells[0].Attribute("t"));
            Assert.Equal("=1+1 & <hat>",cells[0].Descendants(ns+"t").Single().Value);
            Assert.Equal("1234.5678",cells[1].Element(ns+"v")!.Value);
            Assert.Equal("18.5",cells[2].Element(ns+"v")!.Value);
            Assert.Equal("b",(string?)cells[3].Attribute("t"));
            Assert.Equal("1",cells[3].Element(ns+"v")!.Value);
            Assert.Empty(xml.Descendants(ns+"f"));
        }
        finally {directory.Delete(true);}
    }
    [Fact]
    public void TableExport_ShouldPreserveExcelEscapeTokensAndCarriageReturns()
    {
        var directory=Directory.CreateTempSubdirectory("oehs-literal-xlsx-");
        try
        {
            var target=Path.Combine(directory.FullName,"literal.xlsx");
            System.Collections.Generic.IReadOnlyList<object?>[] rows={new object?[]{"_x0041_\rHat"}};
            ExcelDocumentExporter.SaveTable(target,new[]{"Hattın Adı"},rows);
            using var zip=ZipFile.OpenRead(target);
            using var stream=zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
            var xml=XDocument.Load(stream);XNamespace ns="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
            Assert.Equal("_x005F_x0041_\rHat",xml.Descendants(ns+"row").Last().Descendants(ns+"t").Single().Value);
        }
        finally {directory.Delete(true);}
    }

}
