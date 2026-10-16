using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class OehsProgramScopedApplicationDataStoreTests
{
    [Fact]
    public void Save_ShouldPersistEachProgramToItsOwnFile_AndMergeOnLoad()
    {
        var root = Directory.CreateTempSubdirectory("oehs-program-store-");
        try
        {
            var specialContract = Contract(TransportationProgramType.SpecialEducation, "Q1");
            var basicContract = Contract(TransportationProgramType.BasicOrSecondary, "Q1");
            var specialRoute = Route(TransportationProgramType.SpecialEducation, "Q1");
            var basicRoute = Route(TransportationProgramType.BasicOrSecondary, "Q1");
            var specialContractor = Contractor(TransportationProgramType.SpecialEducation);
            var basicContractor = Contractor(TransportationProgramType.BasicOrSecondary);
            var specialPayment = Payment(TransportationProgramType.SpecialEducation, "Q1");
            var basicPayment = Payment(TransportationProgramType.BasicOrSecondary, "Q1");
            var store = new OehsProgramScopedApplicationDataStore(root.FullName);

            store.Save(new OehsApplicationData(
                specialContract,
                new[] { specialRoute, basicRoute },
                new[] { specialContractor, basicContractor },
                new[] { specialPayment, basicPayment },
                new[] { specialContract, basicContract }));

            var specialPath = Path.Combine(root.FullName, "oehs-special-education.json");
            var basicPath = Path.Combine(root.FullName, "oehs-basic-secondary.json");
            Assert.True(File.Exists(specialPath));
            Assert.True(File.Exists(basicPath));

            var savedSpecial = new OehsApplicationDataStore(specialPath).Load();
            var savedBasic = new OehsApplicationDataStore(basicPath).Load();
            Assert.All(savedSpecial.Contracts!, x => Assert.Equal(TransportationProgramType.SpecialEducation, x.ProgramType));
            Assert.All(savedBasic.Contracts!, x => Assert.Equal(TransportationProgramType.BasicOrSecondary, x.ProgramType));
            Assert.All(savedSpecial.ProgressPayments!, x => Assert.Equal(TransportationProgramType.SpecialEducation, x.Input.ProgramType));
            Assert.All(savedBasic.ProgressPayments!, x => Assert.Equal(TransportationProgramType.BasicOrSecondary, x.Input.ProgramType));

            var merged = store.Load();
            Assert.Equal(2, merged.Contracts!.Count);
            Assert.Equal(2, merged.Routes.Count);
            Assert.Equal(2, merged.Contractors.Count);
            Assert.Equal(2, merged.ProgressPayments!.Count);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void Load_ShouldNotImportLegacyDataIntoTheFreshSchema()
    {
        var root = Directory.CreateTempSubdirectory("oehs-program-migration-");
        try
        {
            var legacyContract = Contract(TransportationProgramType.SpecialEducation, "Q1");
            var legacyPath = Path.Combine(root.FullName, "oehs-data.json");
            new OehsApplicationDataStore(legacyPath).Save(new OehsApplicationData(
                legacyContract,
                new[] { Route(TransportationProgramType.SpecialEducation, "Q1") },
                new[] { Contractor(TransportationProgramType.SpecialEducation) },
                new[] { Payment(TransportationProgramType.SpecialEducation, "Q1") },
                new[] { legacyContract }));

            var store = new OehsProgramScopedApplicationDataStore(root.FullName);
            var loaded = store.Load();

            Assert.True(File.Exists(legacyPath));
            Assert.False(File.Exists(Path.Combine(root.FullName, "oehs-special-education.json")));
            Assert.False(File.Exists(Path.Combine(root.FullName, "oehs-basic-secondary.json")));
            Assert.Empty(loaded.Contracts!);
            Assert.Empty(loaded.Routes);
            Assert.Empty(loaded.Contractors);
            Assert.Empty(loaded.ProgressPayments!);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    private static ContractDraftData Contract(TransportationProgramType program, string route) =>
        new("TENDER-SHARED", $"CONTRACT-{route}", $"Work {route}", "Firma", 100000m,
            new DateOnly(2026, 9, 1), new DateOnly(2027, 6, 30), false,
            ContractorTaxOrIdentityNumber: "12345678901", ProgramType: program);

    private static RouteData Route(TransportationProgramType program, string route) =>
        new(route, $"Hat {route}", 100m, TenderNumber: "TENDER-SHARED",
            ContractorTaxOrIdentityNumber: "12345678901", ContractNumber: $"CONTRACT-{route}", ProgramType: program);

    private static Contractor Contractor(TransportationProgramType program) =>
        new("Firma", "12345678901", BankAccountNumber: "TR-TEST", ProgramType: program);

    private static ProgressPaymentHistoryData Payment(TransportationProgramType program, string route)
    {
        var input = new ProgressPaymentInput(1, 100m, 0m, 0m, 100000m, false, 0m, 0m, 0m,
            ProgramType: program);
        return new ProgressPaymentHistoryData(
            Guid.NewGuid(), DateTime.UtcNow, input, ProgressPaymentCalculator.Calculate(input),
            1, new DateOnly(2026, 9, 30), route, $"Hat {route}", $"Work {route}", "Firma",
            "TENDER-SHARED", $"CONTRACT-{route}", ContractorTaxOrIdentityNumber: "12345678901");
    }
}