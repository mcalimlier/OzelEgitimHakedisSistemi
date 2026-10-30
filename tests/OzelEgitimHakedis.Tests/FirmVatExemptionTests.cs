using System.Text.Json;
using OzelEgitimHakedis.Core.Models;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class FirmVatExemptionTests
{
    [Fact]
    public void ContractorVatExemptionTrue_ShouldRoundTrip()
    {
        AssertVatExemptionRoundTrip(true);
    }

    [Fact]
    public void ContractorVatExemptionFalse_ShouldRoundTrip()
    {
        AssertVatExemptionRoundTrip(false);
    }

    private static void AssertVatExemptionRoundTrip(bool vatExempt)
    {
        var firm = new Contractor(
            "Örnek Firma",
            "1234567890",
            ProgramType: TransportationProgramType.BasicOrSecondary,
            VatExempt: vatExempt);

        var json = JsonSerializer.Serialize(firm);
        var restored = JsonSerializer.Deserialize<Contractor>(json);

        Assert.NotNull(restored);
        Assert.Equal<bool?>(vatExempt, restored.VatExempt);
    }

    [Fact]
    public void ContractorJsonWithoutVatExempt_ShouldRemainDistinguishableAsLegacyData()
    {
        const string legacyJson = """
            {
              "Name": "Eski Firma",
              "TaxOrIdentityNumber": "1234567890",
              "Address": "",
              "Phone": "",
              "Email": "",
              "BankAccountNumber": "",
              "ContactPerson": "",
              "ProgramType": 1
            }
            """;

        var restored = JsonSerializer.Deserialize<Contractor>(legacyJson);

        Assert.NotNull(restored);
        Assert.Null(restored.VatExempt);
    }
}
