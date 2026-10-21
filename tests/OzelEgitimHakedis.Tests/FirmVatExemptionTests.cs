using System.Text.Json;
using OzelEgitimHakedis.Core.Models;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class FirmVatExemptionTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ContractorVatExemption_ShouldRoundTrip(bool vatExempt)
    {
        var firm = new Contractor(
            "Örnek Firma",
            "1234567890",
            ProgramType: TransportationProgramType.BasicOrSecondary,
            VatExempt: vatExempt);

        var json = JsonSerializer.Serialize(firm);
        var restored = JsonSerializer.Deserialize<Contractor>(json);

        Assert.NotNull(restored);
        Assert.Equal(vatExempt, restored.VatExempt);
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
